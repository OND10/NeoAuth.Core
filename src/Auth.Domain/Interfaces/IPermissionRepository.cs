using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id);
    Task<Permission?> GetByNameAsync(string name);
    Task<PagedResult<Permission>> GetAllAsync(PaginationFilter filter);
    Task<IEnumerable<Permission>> GetAllAsync();
    Task AddAsync(Permission permission);
    Task UpdateAsync(Permission permission);
    Task DeleteAsync(Guid id);

    // Role permissions
    Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    Task AddRolePermissionAsync(RolePermission rolePermission);
    Task AddRolePermissionsBulkAsync(IEnumerable<RolePermission> rolePermissions);
    Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId);

    // User permissions (direct)
    Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId);
    Task AddUserPermissionAsync(UserPermission userPermission);
    Task AddUserPermissionsBulkAsync(IEnumerable<UserPermission> userPermissions);
    Task RemoveUserPermissionAsync(Guid userId, Guid permissionId);

    // Combined (role + direct user permissions)
    Task<IEnumerable<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? tenantId);
}
