using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IScopeRepository
{
    Task<Scope?> GetByIdAsync(Guid id);
    Task<Scope?> GetByNameAsync(string name);
    Task<PagedResult<Scope>> GetAllAsync(PaginationFilter filter);
    Task<IEnumerable<Scope>> GetAllAsync();
    Task AddAsync(Scope scope);
    Task UpdateAsync(Scope scope);
    Task DeleteAsync(Guid id);
}
