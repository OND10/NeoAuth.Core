using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// Implements IRoleRepository using ASP.NET Identity's RoleManager.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly AuthDbContext _context;

    public RoleRepository(RoleManager<ApplicationRole> roleManager, AuthDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<ApplicationRole?> FindByIdAsync(Guid id)
        => await _roleManager.FindByIdAsync(id.ToString());

    public async Task<ApplicationRole?> FindByNameAsync(string name)
        => await _roleManager.FindByNameAsync(name);

    public async Task<PagedResult<ApplicationRole>> GetAllAsync(PaginationFilter filter)
    {
        var query = _roleManager.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(r => r.Name != null && r.Name.Contains(filter.SearchTerm));
        }

        var totalCount = query.Count();

        var items = await query
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<ApplicationRole>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<(bool Succeeded, string[] Errors)> CreateAsync(ApplicationRole role)
    {
        var result = await _roleManager.CreateAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> UpdateAsync(ApplicationRole role)
    {
        var result = await _roleManager.UpdateAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> DeleteAsync(ApplicationRole role)
    {
        var result = await _roleManager.DeleteAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesByTenantIdAsync(Guid tenantId)
    {
        return await _roleManager.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationRole>> GetRolesByUserIdAsync(Guid userId)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        return await _roleManager.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync();
    }
}
