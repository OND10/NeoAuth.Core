using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IClientRepository
{
    Task<ClientApplication?> GetByClientIdAsync(string clientId);
    Task<ClientApplication?> GetByIdAsync(Guid id);
    Task<PagedResult<ClientApplication>> GetAllAsync(PaginationFilter filter);
    Task AddAsync(ClientApplication client);
    Task UpdateAsync(ClientApplication client);
    Task<IEnumerable<Scope>> GetScopesByClientIdAsync(string clientId);
    Task UpdateScopesAsync(string clientId, IEnumerable<Guid> scopeIds);
}
