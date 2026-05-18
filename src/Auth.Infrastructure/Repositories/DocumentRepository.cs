using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AuthDbContext _context;

    public DocumentRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RequiredDocument>> GetRequiredDocumentsByRolesAsync(IEnumerable<string> roleNames, Guid tenantId)
    {
        // Fetch documents that are either:
        // 1. Not tied to a specific role (TriggerRoleId is null)
        // 2. Tied to one of the user's current roles
        
        return await _context.RequiredDocuments
            .Include(rd => rd.TargetRole)
            .Include(rd => rd.TriggerRole)
            .Where(rd => rd.TenantId == tenantId && rd.IsActive)
            .Where(rd => rd.TriggerRoleId == null || roleNames.Contains(rd.TriggerRole!.Name!))
            .ToListAsync();
    }

    public async Task<IEnumerable<RequiredDocument>> GetRequiredDocumentsForTenantAsync(Guid tenantId)
    {
        return await _context.RequiredDocuments
            .Include(rd => rd.TargetRole)
            .Include(rd => rd.TriggerRole)
            .Where(rd => rd.TenantId == tenantId)
            .ToListAsync();
    }

    public async Task<RequiredDocument?> GetRequiredDocumentByIdAsync(Guid id)
    {
        return await _context.RequiredDocuments
            .Include(rd => rd.TargetRole)
            .Include(rd => rd.TriggerRole)
            .FirstOrDefaultAsync(rd => rd.Id == id);
    }

    public async Task AddRequiredDocumentAsync(RequiredDocument doc)
    {
        await _context.RequiredDocuments.AddAsync(doc);
    }

    public async Task UpdateRequiredDocumentAsync(RequiredDocument doc)
    {
        _context.RequiredDocuments.Update(doc);
        await Task.CompletedTask;
    }

    public async Task DeleteRequiredDocumentAsync(Guid id)
    {
        var doc = await _context.RequiredDocuments.FindAsync(id);
        if (doc != null)
        {
            _context.RequiredDocuments.Remove(doc);
        }
    }

    public async Task<VerificationRequest?> GetVerificationRequestByIdAsync(Guid id)
    {
        return await _context.VerificationRequests
            .Include(x => x.RequiredDocument)
            .Include(x => x.User)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<VerificationRequest>> GetPendingRequestsAsync()
    {
        return await _context.VerificationRequests
            .Include(x => x.Documents)
            .Include(x => x.RequiredDocument)
            .Include(x => x.User)
            .Where(x => x.Status == VerificationStatus.Pending)
            .ToListAsync();
    }

    public async Task<VerificationRequest?> GetLatestUserRequestAsync(Guid userId, Guid tenantId)
    {
        return await _context.VerificationRequests
            .Include(x => x.Documents)
            .Include(x => x.RequiredDocument)
            .Where(x => x.UserId == userId && x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddVerificationRequestAsync(VerificationRequest request)
    {
        await _context.VerificationRequests.AddAsync(request);
    }

    public async Task UpdateVerificationRequestAsync(VerificationRequest request)
    {
        _context.VerificationRequests.Update(request);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
