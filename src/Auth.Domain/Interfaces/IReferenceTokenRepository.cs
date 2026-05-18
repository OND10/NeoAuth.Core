using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IReferenceTokenRepository
{
    Task AddAsync(ReferenceToken token);
    Task<ReferenceToken?> GetByTokenAsync(string token);
    Task UpdateAsync(ReferenceToken token);
    Task RevokeByClientIdAsync(Guid clientId);
}
