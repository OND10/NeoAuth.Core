using Auth.Application.DTOs;
using Auth.Domain.Common;

namespace Auth.Application.Interfaces;

public interface IDocumentService
{
    // Required Documents (Admin & User)
    Task<Result<IEnumerable<RequiredDocumentDto>>> GetRequiredDocumentsAsync(IEnumerable<string> roleNames);
    Task<Result<IEnumerable<RequiredDocumentDto>>> GetTenantRequiredDocumentsAsync();
    Task<Result<RequiredDocumentDto>> CreateRequiredDocumentAsync(ConfigureRequiredDocumentRequest request);
    Task<Result> UpdateRequiredDocumentAsync(Guid id, ConfigureRequiredDocumentRequest request);
    Task<Result> DeleteRequiredDocumentAsync(Guid id);

    // Verification Workflow
    Task<Result<Guid>> CreateVerificationRequestAsync(Guid userId, Guid tenantId, CreateVerificationRequest request);
    Task<Result> SubmitBulkVerificationAsync(Guid userId, Guid tenantId, BulkVerificationRequest request);
    Task<Result> SubmitBulkProcessedAsync(Guid userId, Guid tenantId, BulkProcessedVerificationRequest request);
    Task<Result> ReviewRequestAsync(Guid requestId, ReviewVerificationRequest review, Guid adminId);
    Task<Result<IEnumerable<VerificationRequestDto>>> GetPendingRequestsAsync();
    Task<Result<VerificationRequestDto>> GetUserStatusAsync(Guid userId, Guid tenantId);
}
