using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly AuthDbContext _context;

    public PermissionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(Guid id)
    {
        return await _context.Permissions.FindAsync(id);
    }

    public async Task<Permission?> GetByNameAsync(string name)
    {
        return await _context.Permissions.FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<PagedResult<Permission>> GetAllAsync(PaginationFilter filter)
    {
        var query = _context.Permissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(filter.SearchTerm) || (p.Description != null && p.Description.Contains(filter.SearchTerm)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Permission>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions.ToListAsync();
    }

    public async Task AddAsync(Permission permission)
    {
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Permission permission)
    {
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var permission = await _context.Permissions.FindAsync(id);
        if (permission is not null)
        {
            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
        }
    }

    // ──── Role Permissions ────

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task AddRolePermissionAsync(RolePermission rolePermission)
    {
        var exists = await _context.RolePermissions
            .AnyAsync(rp => rp.RoleId == rolePermission.RoleId && rp.PermissionId == rolePermission.PermissionId);

        if (!exists)
        {
            await _context.RolePermissions.AddAsync(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddRolePermissionsBulkAsync(IEnumerable<RolePermission> rolePermissions)
    {
        var listToInsert = new List<RolePermission>();
        foreach (var rp in rolePermissions)
        {
            var exists = await _context.RolePermissions.AnyAsync(x => x.RoleId == rp.RoleId && x.PermissionId == rp.PermissionId);
            if (!exists) listToInsert.Add(rp);
        }

        if (listToInsert.Any())
        {
            await _context.RolePermissions.AddRangeAsync(listToInsert);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveRolePermissionAsync(Guid roleId, Guid permissionId)
    {
        var rp = await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

        if (rp is not null)
        {
            _context.RolePermissions.Remove(rp);
            await _context.SaveChangesAsync();
        }
    }

    // ──── User Permissions (Direct) ────

    public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId)
    {
        return await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Include(up => up.Permission)
            .Select(up => up.Permission)
            .ToListAsync();
    }

    public async Task AddUserPermissionAsync(UserPermission userPermission)
    {
        var exists = await _context.UserPermissions
            .AnyAsync(up => up.UserId == userPermission.UserId && up.PermissionId == userPermission.PermissionId);

        if (!exists)
        {
            await _context.UserPermissions.AddAsync(userPermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddUserPermissionsBulkAsync(IEnumerable<UserPermission> userPermissions)
    {
        var listToInsert = new List<UserPermission>();
        foreach (var up in userPermissions)
        {
            var exists = await _context.UserPermissions.AnyAsync(x => x.UserId == up.UserId && x.PermissionId == up.PermissionId);
            if (!exists) listToInsert.Add(up);
        }

        if (listToInsert.Any())
        {
            await _context.UserPermissions.AddRangeAsync(listToInsert);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveUserPermissionAsync(Guid userId, Guid permissionId)
    {
        var up = await _context.UserPermissions
            .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

        if (up is not null)
        {
            _context.UserPermissions.Remove(up);
            await _context.SaveChangesAsync();
        }
    }

    // ──── Combined (Role + Direct User Permissions) ────

    public async Task<IEnumerable<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? tenantId)
    {
        // Get user's roles (filtered by tenant if applicable)
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        // If tenant-scoped, filter roles to those belonging to the tenant (or global roles)
        if (tenantId.HasValue)
        {
            var tenantRoleIds = await _context.Roles
                .Where(r => roleIds.Contains(r.Id) && (r.TenantId == null || r.TenantId == tenantId))
                .Select(r => r.Id)
                .ToListAsync();
            roleIds = tenantRoleIds;
        }

        // Get permissions from roles
        var rolePermissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .ToListAsync();

        // Get direct user permissions
        var userPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .ToListAsync();

        // Combine and deduplicate
        return rolePermissions.Union(userPermissions).DistinctBy(p => p.Id).ToList();
    }
}
