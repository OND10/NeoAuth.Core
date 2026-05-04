using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClaimsCacheService _cache;

    public PermissionService(
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IClaimsCacheService cache)
    {
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<Result<PermissionResponse>> CreateAsync(CreatePermissionRequest request)
    {
        var existing = await _permissionRepository.GetByNameAsync(request.Name);
        if (existing is not null)
            return Result.Failure<PermissionResponse>(new Error("Permission.AlreadyExists", $"Permission '{request.Name}' already exists."));

        if (request.ParentId.HasValue)
        {
            var parent = await _permissionRepository.GetByIdAsync(request.ParentId.Value);
            if (parent is null)
                return Result.Failure<PermissionResponse>(Error.PermissionNotFound);
        }

        var permission = new Permission
        {
            Name = request.Name,
            Description = request.Description,
            ParentId = request.ParentId
        };

        await _permissionRepository.AddAsync(permission);

        return Result.Success(new PermissionResponse(permission.Id, permission.Name, permission.Description, permission.ParentId), "permission created successfully");
    }

    public async Task<Result<PermissionResponse>> UpdateAsync(Guid id, UpdatePermissionRequest request)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission is null)
            return Result.Failure<PermissionResponse>(Error.PermissionNotFound);

        // Name change validation
        if (permission.Name != request.Name)
        {
            var existing = await _permissionRepository.GetByNameAsync(request.Name);
            if (existing is not null)
                return Result.Failure<PermissionResponse>(new Error("Permission.AlreadyExists", $"Permission '{request.Name}' already exists."));
        }

        // Circular dependency check
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == id)
                return Result.Failure<PermissionResponse>(new Error("Permission.CircularDependency", "A permission cannot be its own parent."));

            if (await IsCircularDependency(id, request.ParentId.Value))
                return Result.Failure<PermissionResponse>(new Error("Permission.CircularDependency", "Circular dependency detected in permission hierarchy."));
        }

        permission.Name = request.Name;
        permission.Description = request.Description;
        permission.ParentId = request.ParentId;

        await _permissionRepository.UpdateAsync(permission);

        return Result.Success(new PermissionResponse(permission.Id, permission.Name, permission.Description, permission.ParentId), "permission updated successfully");
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var permission = await _permissionRepository.GetByIdAsync(id);
        if (permission is null)
            return Result.Failure(Error.PermissionNotFound);

        await _permissionRepository.DeleteAsync(id);
        return Result.Success("Permission deleted successfully.");
    }

    private async Task<bool> IsCircularDependency(Guid permissionId, Guid potentialParentId)
    {
        var currentParentId = potentialParentId;
        while (currentParentId != Guid.Empty)
        {
            var parent = await _permissionRepository.GetByIdAsync(currentParentId);
            if (parent == null || parent.ParentId == null)
                break;

            if (parent.ParentId == permissionId)
                return true;

            currentParentId = parent.ParentId.Value;
        }

        return false;
    }

    public async Task<Result<PagedResult<PermissionResponse>>> GetAllAsync(PaginationFilter filter)
    {
        var pagedPermissions = await _permissionRepository.GetAllAsync(filter);
        var responseItems = pagedPermissions.Items.Select(p => new PermissionResponse(p.Id, p.Name, p.Description, p.ParentId));
        var response = new PagedResult<PermissionResponse>(responseItems, pagedPermissions.TotalCount, pagedPermissions.PageNumber, pagedPermissions.PageSize);
        return Result.Success(response, "Permissions fetched successfully");
    }

    public async Task<Result> AssignToRoleAsync(Guid roleId, Guid permissionId)
    {
        var role = await _roleRepository.FindByIdAsync(roleId);
        if (role is null)
            return Result.Failure(Error.RoleNotFound);

        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission is null)
            return Result.Failure(Error.PermissionNotFound);

        await _permissionRepository.AddRolePermissionAsync(new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        });

        return Result.Success("Permission assigned to role successfully");
    }

    public async Task<Result> RemoveFromRoleAsync(Guid roleId, Guid permissionId)
    {
        await _permissionRepository.RemoveRolePermissionAsync(roleId, permissionId);
        return Result.Success("Permission removed from role successfully");
    }

    public async Task<Result> AssignToRoleBulkAsync(Guid roleId, List<Guid> permissionIds)
    {
        var role = await _roleRepository.FindByIdAsync(roleId);
        if (role is null)
            return Result.Failure(Error.RoleNotFound);

        var rolePermissions = permissionIds.Select(pid => new RolePermission
        {
            RoleId = roleId,
            PermissionId = pid
        });

        await _permissionRepository.AddRolePermissionsBulkAsync(rolePermissions);
        return Result.Success("Permissions assigned to role successfully");
    }

    public async Task<Result> AssignToUserAsync(Guid userId, Guid permissionId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(Error.UserNotFound);

        var permission = await _permissionRepository.GetByIdAsync(permissionId);
        if (permission is null)
            return Result.Failure(Error.PermissionNotFound);

        await _permissionRepository.AddUserPermissionAsync(new UserPermission
        {
            UserId = userId,
            PermissionId = permissionId
        });

        // Invalidate cache for this user
        await _cache.InvalidateUserCacheAsync(userId);

        return Result.Success("Permission assigned to user successfully");
    }

    public async Task<Result> RemoveFromUserAsync(Guid userId, Guid permissionId)
    {
        await _permissionRepository.RemoveUserPermissionAsync(userId, permissionId);
        await _cache.InvalidateUserCacheAsync(userId);
        return Result.Success("Permission removed from user successfully");
    }

    public async Task<Result> AssignToUserBulkAsync(Guid userId, List<Guid> permissionIds)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(Error.UserNotFound);

        var userPermissions = permissionIds.Select(pid => new UserPermission
        {
            UserId = userId,
            PermissionId = pid
        });

        await _permissionRepository.AddUserPermissionsBulkAsync(userPermissions);
        await _cache.InvalidateUserCacheAsync(userId);
        return Result.Success("Permission assigned to user successfully");
    }
}
