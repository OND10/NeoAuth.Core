using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AuthDbContext _context;

    public TenantRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants.FindAsync(id);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _context.Tenants.ToListAsync();
    }

    public async Task AddAsync(Tenant tenant)
    {
        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Tenant>> GetTenantsByUserIdAsync(Guid userId)
    {
        return await _context.UserTenants
            .Where(ut => ut.UserId == userId)
            .Include(ut => ut.Tenant)
            .Select(ut => ut.Tenant)
            .ToListAsync();
    }

    public async Task AddUserToTenantAsync(UserTenant userTenant)
    {
        await _context.UserTenants.AddAsync(userTenant);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromTenantAsync(Guid userId, Guid tenantId)
    {
        var userTenant = await _context.UserTenants
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TenantId == tenantId);

        if (userTenant is not null)
        {
            _context.UserTenants.Remove(userTenant);
            await _context.SaveChangesAsync();
        }
    }
}
