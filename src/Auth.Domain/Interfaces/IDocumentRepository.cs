using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IDocumentRepository
{
    // Required Documents
    Task<IEnumerable<RequiredDocument>> GetRequiredDocumentsByRolesAsync(IEnumerable<string> roleNames, Guid tenantId);
    Task<IEnumerable<RequiredDocument>> GetRequiredDocumentsForTenantAsync(Guid tenantId);
    Task<RequiredDocument?> GetRequiredDocumentByIdAsync(Guid id);
    Task<IEnumerable<RequiredDocument>> GetRequiredDocuments();
    Task AddRequiredDocumentAsync(RequiredDocument doc);
    Task UpdateRequiredDocumentAsync(RequiredDocument doc);
    Task DeleteRequiredDocumentAsync(Guid id);
    
    // Verification Requests
    Task<VerificationRequest?> GetVerificationRequestByIdAsync(Guid id);
    Task<IEnumerable<VerificationRequest>> GetPendingRequestsAsync();
    Task<VerificationRequest?> GetLatestUserRequestAsync(Guid userId, Guid tenantId);
    Task AddVerificationRequestAsync(VerificationRequest request);
    Task UpdateVerificationRequestAsync(VerificationRequest request);
    
    Task SaveChangesAsync();
}
