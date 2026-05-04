using Auth.Application.DTOs;
using Auth.Domain.Common;

namespace Auth.Application.Interfaces;

public interface ITenantService
{
    Task<Result<TenantResponse>> CreateAsync(CreateTenantRequest request);
    Task<Result<IEnumerable<TenantResponse>>> GetAllAsync();
    Task<Result<TenantResponse>> GetByIdAsync(Guid id);
    Task<Result> AssignUserAsync(Guid tenantId, AssignUserToTenantRequest request);
    Task<Result> RemoveUserAsync(Guid tenantId, Guid userId);
    Task<Result<IEnumerable<TenantResponse>>> GetUserTenantsAsync(Guid userId);
}
