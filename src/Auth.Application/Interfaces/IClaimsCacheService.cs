namespace Auth.Application.Interfaces;

/// <summary>
/// Interface for caching claims data (permissions, scopes).
/// Default implementation uses IMemoryCache.
/// </summary>
public interface IClaimsCacheService
{
    Task<IList<string>?> GetPermissionsAsync(Guid userId, Guid? tenantId);
    Task SetPermissionsAsync(Guid userId, Guid? tenantId, IList<string> permissions, TimeSpan? expiration = null);
    Task<IList<string>?> GetScopesAsync(string clientId);
    Task SetScopesAsync(string clientId, IList<string> scopes, TimeSpan? expiration = null);
    Task InvalidateUserCacheAsync(Guid userId);
    Task InvalidateClientCacheAsync(string clientId);
}
