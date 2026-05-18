using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AuthDbContext _context;

    public DeviceRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserDevice?> GetByIdAsync(Guid deviceId)
        => await _context.UserDevices.FirstOrDefaultAsync(d => d.Id == deviceId);

    public async Task<UserDevice?> GetByFingerprintAsync(Guid userId, string fingerprintHash)
        => await _context.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId
                                   && d.DeviceFingerprint == fingerprintHash
                                   && d.Status == DeviceStatus.Active);

    public async Task<IEnumerable<UserDevice>> GetActiveDevicesByUserAsync(Guid userId)
        => await _context.UserDevices
            .Where(d => d.UserId == userId && d.Status == DeviceStatus.Active)
            .OrderByDescending(d => d.LastActivityAt)
            .ToListAsync();

    public async Task<int> GetActiveDeviceCountAsync(Guid userId)
        => await _context.UserDevices
            .CountAsync(d => d.UserId == userId && d.Status == DeviceStatus.Active);

    public async Task<RoleDeviceLimit?> GetRoleDeviceLimitAsync(Guid roleId)
        => await _context.RoleDeviceLimits
            .FirstOrDefaultAsync(r => r.RoleId == roleId);

    public async Task<UserDevice?> GetOldestActiveDeviceAsync(Guid userId)
        => await _context.UserDevices
            .Where(d => d.UserId == userId && d.Status == DeviceStatus.Active)
            .OrderBy(d => d.LastActivityAt)
            .FirstOrDefaultAsync();

    public async Task AddAsync(UserDevice device)
        => await _context.UserDevices.AddAsync(device);

    public async Task UpdateAsync(UserDevice device)
    {
        _context.UserDevices.Update(device);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
