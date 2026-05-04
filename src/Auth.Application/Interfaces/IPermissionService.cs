using Auth.Application.DTOs;
using Auth.Domain.Common;

namespace Auth.Application.Interfaces;

public interface IPermissionService
{
    Task<Result<PermissionResponse>> CreateAsync(CreatePermissionRequest request);
    Task<Result<PagedResult<PermissionResponse>>> GetAllAsync(PaginationFilter filter);
    Task<Result> AssignToRoleAsync(Guid roleId, Guid permissionId);
    Task<Result> AssignToRoleBulkAsync(Guid roleId, List<Guid> permissionIds);
    Task<Result> RemoveFromRoleAsync(Guid roleId, Guid permissionId);
    Task<Result> AssignToUserAsync(Guid userId, Guid permissionId);
    Task<Result> AssignToUserBulkAsync(Guid userId, List<Guid> permissionIds);
    Task<Result<PermissionResponse>> UpdateAsync(Guid id, UpdatePermissionRequest request);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> RemoveFromUserAsync(Guid userId, Guid permissionId);
}
