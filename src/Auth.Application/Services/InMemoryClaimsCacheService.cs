using Auth.Application.Interfaces;
using Auth.Application.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Auth.Application.Services;

public class InMemoryClaimsCacheService : IClaimsCacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;

    public InMemoryClaimsCacheService(IMemoryCache cache, IOptions<AuthOptions> options)
    {
        _cache = cache;
        _defaultExpiration = TimeSpan.FromMinutes(options.Value.CacheExpirationMinutes);
    }

    public Task<IList<string>?> GetPermissionsAsync(Guid userId, Guid? tenantId)
    {
        var key = $"permissions:{userId}:{tenantId}";
        _cache.TryGetValue(key, out IList<string>? permissions);
        return Task.FromResult(permissions);
    }

    public Task SetPermissionsAsync(Guid userId, Guid? tenantId, IList<string> permissions, TimeSpan? expiration = null)
    {
        var key = $"permissions:{userId}:{tenantId}";
        _cache.Set(key, permissions, expiration ?? _defaultExpiration);
        return Task.CompletedTask;
    }

    public Task<IList<string>?> GetScopesAsync(string clientId)
    {
        var key = $"scopes:{clientId}";
        _cache.TryGetValue(key, out IList<string>? scopes);
        return Task.FromResult(scopes);
    }

    public Task SetScopesAsync(string clientId, IList<string> scopes, TimeSpan? expiration = null)
    {
        var key = $"scopes:{clientId}";
        _cache.Set(key, scopes, expiration ?? _defaultExpiration);
        return Task.CompletedTask;
    }

    public Task InvalidateUserCacheAsync(Guid userId)
    {
        // IMemoryCache doesn't support wildcard removal, so we remove known patterns
        // In production with Redis, you'd use pattern-based key deletion
        _cache.Remove($"permissions:{userId}:");
        return Task.CompletedTask;
    }

    public Task InvalidateClientCacheAsync(string clientId)
    {
        _cache.Remove($"scopes:{clientId}");
        return Task.CompletedTask;
    }
}
