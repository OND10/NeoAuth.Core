using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AuthDbContext _context;

    public ClientRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<ClientApplication?> GetByClientIdAsync(string clientId)
    {
        return await _context.ClientApplications
            .Include(c => c.AllowedScopes)
                .ThenInclude(cs => cs.Scope)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
    }

    public async Task<ClientApplication?> GetByIdAsync(Guid id)
    {
        return await _context.ClientApplications
            .Include(c => c.AllowedScopes)
                .ThenInclude(cs => cs.Scope)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PagedResult<ClientApplication>> GetAllAsync(PaginationFilter filter)
    {
        var query = _context.ClientApplications
            .Include(c => c.AllowedScopes)
                .ThenInclude(cs => cs.Scope)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(c => c.Name.Contains(filter.SearchTerm) || c.ClientId.Contains(filter.SearchTerm));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<ClientApplication>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task AddAsync(ClientApplication client)
    {
        await _context.ClientApplications.AddAsync(client);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ClientApplication client)
    {
        _context.ClientApplications.Update(client);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Scope>> GetScopesByClientIdAsync(string clientId)
    {
        return await _context.ClientScopes
            .Include(cs => cs.Scope)
            .Where(cs => cs.ClientApplication.ClientId == clientId)
            .Select(cs => cs.Scope)
            .ToListAsync();
    }

    public async Task UpdateScopesAsync(string clientId, IEnumerable<Guid> scopeIds)
    {
        var client = await _context.ClientApplications
            .Include(c => c.AllowedScopes)
            .FirstOrDefaultAsync(c => c.ClientId == clientId);

        if (client is not null)
        {
            var validScopeIds = await _context.Scopes
                .Where(s => scopeIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            _context.ClientScopes.RemoveRange(client.AllowedScopes);
            client.AllowedScopes = validScopeIds.Select(sid => new ClientScope { ScopeId = sid, ClientApplicationId = client.Id }).ToList();
            await _context.SaveChangesAsync();
        }
    }
}
