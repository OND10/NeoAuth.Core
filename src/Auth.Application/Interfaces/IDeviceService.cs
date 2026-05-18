using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IDeviceService
{
    /// <summary>
    /// Registers or updates a device during login.
    /// Returns the device entity so the caller can link it to the refresh token.
    /// Returns failure if the device limit is exceeded and policy is Reject.
    /// </summary>
    Task<Result<UserDevice>> RegisterOrUpdateDeviceAsync(
        Guid userId, DeviceInfo deviceInfo, string? ipAddress, Guid? tenantId);

    /// <summary>
    /// Validates that a device is still active during token refresh.
    /// Updates LastActivityAt on success.
    /// </summary>
    Task<Result<UserDevice>> ValidateDeviceAsync(Guid userId, string deviceFingerprint);

    Task<Result<IEnumerable<DeviceResponse>>> GetUserDevicesAsync(Guid userId);
    Task<Result> RevokeDeviceAsync(Guid userId, Guid deviceId);
    Task<Result> RenameDeviceAsync(Guid userId, Guid deviceId, string friendlyName);

    /// <summary>Activates a device after successful OTP verification.</summary>
    Task<Result> ActivateDeviceAsync(Guid userId, Guid deviceId);

    /// <summary>Admin: force-revoke any device.</summary>
    Task<Result> AdminRevokeDeviceAsync(Guid deviceId);
}
