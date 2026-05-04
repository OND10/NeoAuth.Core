using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Abstracts role management operations (wraps ASP.NET Identity RoleManager).
/// Implementation lives in Infrastructure layer.
/// </summary>
public interface IRoleRepository
{
    Task<ApplicationRole?> FindByIdAsync(Guid id);
    Task<ApplicationRole?> FindByNameAsync(string name);
    Task<PagedResult<ApplicationRole>> GetAllAsync(PaginationFilter filter);
    Task<(bool Succeeded, string[] Errors)> CreateAsync(ApplicationRole role);
    Task<(bool Succeeded, string[] Errors)> UpdateAsync(ApplicationRole role);
    Task<(bool Succeeded, string[] Errors)> DeleteAsync(ApplicationRole role);

    Task<IEnumerable<ApplicationRole>> GetRolesByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<ApplicationRole>> GetRolesByUserIdAsync(Guid userId);
}
