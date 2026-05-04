using Auth.Application.DTOs;
using Auth.Domain.Common;

namespace Auth.Application.Interfaces;

public interface IScopeService
{
    Task<Result<ScopeResponse>> CreateAsync(CreateScopeRequest request);
    Task<Result<PagedResult<ScopeResponse>>> GetAllAsync(PaginationFilter filter);
    Task<Result<ScopeResponse>> GetByIdAsync(Guid id);
    Task<Result> UpdateAsync(Guid id, UpdateScopeRequest request);
    Task<Result> DeleteAsync(Guid id);
}
