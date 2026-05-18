using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class ReferenceTokenRepository : IReferenceTokenRepository
{
    private readonly AuthDbContext _context;

    public ReferenceTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ReferenceToken token)
    {
        _context.ReferenceTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<ReferenceToken?> GetByTokenAsync(string token)
    {
        return await _context.ReferenceTokens
            .Include(t => t.Client)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
    }

    public async Task UpdateAsync(ReferenceToken token)
    {
        _context.ReferenceTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeByClientIdAsync(Guid clientId)
    {
        var tokens = await _context.ReferenceTokens
            .Where(t => t.ClientId == clientId && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync();
    }
}
