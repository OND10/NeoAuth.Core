using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Enums;

namespace Auth.Domain.Interfaces;

public interface IDeviceRepository
{
    Task<UserDevice?> GetByIdAsync(Guid deviceId);
    Task<UserDevice?> GetByFingerprintAsync(Guid userId, string fingerprintHash);
    Task<IEnumerable<UserDevice>> GetActiveDevicesByUserAsync(Guid userId);
    Task<int> GetActiveDeviceCountAsync(Guid userId);
    Task<RoleDeviceLimit?> GetRoleDeviceLimitAsync(Guid roleId);
    Task<UserDevice?> GetOldestActiveDeviceAsync(Guid userId);

    Task AddAsync(UserDevice device);
    Task UpdateAsync(UserDevice device);
    Task SaveChangesAsync();
}
