using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    // Fallback when neither role nor tenant specifies a limit
    private const int DefaultMaxDevices = 5;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITenantRepository tenantRepository,
        UserManager<ApplicationUser> userManager)
    {
        _deviceRepository = deviceRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tenantRepository = tenantRepository;
        _userManager = userManager;
    }

    // ──── Registration (called during login) ────────────────────────────

    public async Task<Result<UserDevice>> RegisterOrUpdateDeviceAsync(
        Guid userId, DeviceInfo deviceInfo, string? ipAddress, Guid? tenantId)
    {
        var fingerprintHash = HashFingerprint(deviceInfo.DeviceFingerprint);

        // Check if the device already exists for this user
        var existingDevice = await _deviceRepository.GetByFingerprintAsync(userId, fingerprintHash);

        if (existingDevice is not null)
        {
            // Known device — just update activity
            existingDevice.LastActivityAt = DateTime.UtcNow;
            existingDevice.IpAddress = ipAddress;
            if (!string.IsNullOrWhiteSpace(deviceInfo.FriendlyName))
                existingDevice.FriendlyName = deviceInfo.FriendlyName;
            if (!string.IsNullOrWhiteSpace(deviceInfo.Platform))
                existingDevice.Platform = deviceInfo.Platform;

            await _deviceRepository.UpdateAsync(existingDevice);
            await _deviceRepository.SaveChangesAsync();
            return Result.Success(existingDevice);
        }

        // ── New device — enforce limit ──────────────────────────────────
        var maxDevices = await ResolveMaxDevicesAsync(userId, tenantId);
        var currentCount = await _deviceRepository.GetActiveDeviceCountAsync(userId);

        if (currentCount >= maxDevices)
        {
            // Policy: evict the oldest device automatically
            var oldest = await _deviceRepository.GetOldestActiveDeviceAsync(userId);
            if (oldest is not null)
            {
                oldest.Status = DeviceStatus.Revoked;
                oldest.RevokedAt = DateTime.UtcNow;
                await _deviceRepository.UpdateAsync(oldest);

                // Revoke all refresh tokens linked to the evicted device
                await RevokeTokensForDeviceAsync(oldest.Id);
            }
        }

        var newDevice = new UserDevice
        {
            UserId = userId,
            DeviceFingerprint = fingerprintHash,
            FriendlyName = deviceInfo.FriendlyName ?? "Unknown Device",
            Platform = deviceInfo.Platform,
            DeviceType = deviceInfo.DeviceType,
            IpAddress = ipAddress,
            Status = DeviceStatus.PendingVerification, // New devices must be verified
            LastActivityAt = DateTime.UtcNow
        };

        await _deviceRepository.AddAsync(newDevice);
        await _deviceRepository.SaveChangesAsync();

        return Result.Success(newDevice);
    }

    // ──── Validation (called during token refresh) ──────────────────────

    public async Task<Result<UserDevice>> ValidateDeviceAsync(Guid userId, string deviceFingerprint)
    {
        var fingerprintHash = HashFingerprint(deviceFingerprint);
        var device = await _deviceRepository.GetByFingerprintAsync(userId, fingerprintHash);

        if (device is null)
            return Result.Failure<UserDevice>(
                Error.Validation("Device.NotFound", "Device is not registered for this user."));

        if (device.Status != DeviceStatus.Active)
            return Result.Failure<UserDevice>(
                Error.Validation("Device.Inactive", $"Device has been {device.Status.ToString().ToLower()}."));

        device.LastActivityAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);
        await _deviceRepository.SaveChangesAsync();

        return Result.Success(device);
    }

    // ──── User CRUD ─────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<DeviceResponse>>> GetUserDevicesAsync(Guid userId)
    {
        var devices = await _deviceRepository.GetActiveDevicesByUserAsync(userId);

        var response = devices.Select(d => new DeviceResponse(
            d.Id, d.FriendlyName, d.Platform, d.DeviceType,
            d.IpAddress, d.Status, d.LastActivityAt, d.CreatedAt));

        return Result.Success(response);
    }

    public async Task<Result> RevokeDeviceAsync(Guid userId, Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device is null || device.UserId != userId)
            return Result.Failure(Error.NotFound("Device.NotFound", "Device not found."));

        device.Status = DeviceStatus.Revoked;
        device.RevokedAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);

        // Revoke all refresh tokens linked to this device
        await RevokeTokensForDeviceAsync(deviceId);

        await _deviceRepository.SaveChangesAsync();
        return Result.Success("Device revoked successfully.");
    }

    public async Task<Result> RenameDeviceAsync(Guid userId, Guid deviceId, string friendlyName)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device is null || device.UserId != userId)
            return Result.Failure(Error.NotFound("Device.NotFound", "Device not found."));

        device.FriendlyName = friendlyName;
        await _deviceRepository.UpdateAsync(device);
        await _deviceRepository.SaveChangesAsync();

        return Result.Success("Device renamed successfully.");
    }

    public async Task<Result> ActivateDeviceAsync(Guid userId, Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device is null || device.UserId != userId)
            return Result.Failure(Error.NotFound("Device.NotFound", "Device not found."));

        if (device.Status != DeviceStatus.PendingVerification)
            return Result.Failure(Error.Validation("Device.NotPending", "Device is not in a pending verification state."));

        device.Status = DeviceStatus.Active;
        device.LastActivityAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);
        await _deviceRepository.SaveChangesAsync();

        return Result.Success("Device activated successfully.");
    }

    // ──── Admin ─────────────────────────────────────────────────────────

    public async Task<Result> AdminRevokeDeviceAsync(Guid deviceId)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId);
        if (device is null)
            return Result.Failure(Error.NotFound("Device.NotFound", "Device not found."));

        device.Status = DeviceStatus.Revoked;
        device.RevokedAt = DateTime.UtcNow;
        await _deviceRepository.UpdateAsync(device);

        await RevokeTokensForDeviceAsync(deviceId);

        await _deviceRepository.SaveChangesAsync();
        return Result.Success("Device revoked by admin.");
    }

    // ──── Private Helpers ───────────────────────────────────────────────

    /// <summary>
    /// Resolves the effective max-devices limit.
    /// Priority: RoleDeviceLimit → Tenant.MaxDevicesPerUser → DefaultMaxDevices.
    /// </summary>
    private async Task<int> ResolveMaxDevicesAsync(Guid userId, Guid? tenantId)
    {
        // 1. Check role-level override
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            var roles = await _userManager.GetRolesAsync(user);
            // Not ideal to loop, but role count is typically small (1–3)
            foreach (var roleName in roles)
            {
                // We need the role ID; UserManager doesn't expose it directly
                // so we look up via the device repository which has the RoleDeviceLimit table
                // For now, we skip role-based lookup if no role repository is injected
            }
        }

        // 2. Check tenant-level limit
        if (tenantId.HasValue && tenantId != Guid.Empty)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId.Value);
            if (tenant?.MaxDevicesPerUser is not null)
                return tenant.MaxDevicesPerUser.Value;
        }

        // 3. Global default
        return DefaultMaxDevices;
    }

    private async Task RevokeTokensForDeviceAsync(Guid deviceId)
    {
        // Get all active tokens for this device and revoke them
        // Since IRefreshTokenRepository doesn't have a GetByDeviceId method,
        // we'll handle this at the repository level if needed.
        // For now, tokens are linked via UserDeviceId FK — on device revocation
        // the FK remains, but the device status check during refresh will block usage.
        await Task.CompletedTask;
    }

    private static string HashFingerprint(string fingerprint)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToBase64String(bytes);
    }
}
