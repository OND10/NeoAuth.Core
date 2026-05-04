using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySubdomainAsync(string subdomain);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task<IEnumerable<Tenant>> GetTenantsByUserIdAsync(Guid userId);
    Task AddUserToTenantAsync(UserTenant userTenant);
    Task RemoveUserFromTenantAsync(Guid userId, Guid tenantId);
}
