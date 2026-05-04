using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class ScopeRepository : IScopeRepository
{
    private readonly AuthDbContext _context;

    public ScopeRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Scope?> GetByIdAsync(Guid id)
    {
        return await _context.Scopes.FindAsync(id);
    }

    public async Task<Scope?> GetByNameAsync(string name)
    {
        return await _context.Scopes.FirstOrDefaultAsync(s => s.Name == name);
    }

    public async Task<PagedResult<Scope>> GetAllAsync(PaginationFilter filter)
    {
        var query = _context.Scopes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(s => s.Name.Contains(filter.SearchTerm) || (s.Description != null && s.Description.Contains(filter.SearchTerm)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<Scope>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<IEnumerable<Scope>> GetAllAsync()
    {
        return await _context.Scopes.ToListAsync();
    }

    public async Task AddAsync(Scope scope)
    {
        await _context.Scopes.AddAsync(scope);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Scope scope)
    {
        _context.Scopes.Update(scope);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var scope = await _context.Scopes.FindAsync(id);
        if (scope is not null)
        {
            _context.Scopes.Remove(scope);
            await _context.SaveChangesAsync();
        }
    }
}
