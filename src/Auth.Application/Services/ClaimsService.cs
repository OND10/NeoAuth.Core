using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class ClaimsService : IClaimsService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IScopeRepository _scopeRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IClaimsCacheService _cache;

    public ClaimsService(
        IPermissionRepository permissionRepository,
        IScopeRepository scopeRepository,
        IClientRepository clientRepository,
        IClaimsCacheService cache)
    {
        _permissionRepository = permissionRepository;
        _scopeRepository = scopeRepository;
        _clientRepository = clientRepository;
        _cache = cache;
    }

    public async Task<Result<IList<string>>> GetUserPermissionsAsync(Guid userId, Guid? tenantId)
    {
        var cached = await _cache.GetPermissionsAsync(userId, tenantId);
        if (cached is not null) return Result.Success<IList<string>>(cached);

        var assignedPermissions = await _permissionRepository.GetEffectivePermissionsAsync(userId, tenantId);
        var allPermissions = await _permissionRepository.GetAllAsync();

        var expanded = ExpandHierarchy(assignedPermissions, allPermissions, p => p.Id, p => p.ParentId, p => p.Name);

        await _cache.SetPermissionsAsync(userId, tenantId, expanded);
        return Result.Success<IList<string>>(expanded);
    }

    public async Task<Result<IList<string>>> GetClientScopesAsync(string clientId)
    {
        var cached = await _cache.GetScopesAsync(clientId);
        if (cached is not null) return Result.Success<IList<string>>(cached);

        var assignedScopes = await _clientRepository.GetScopesByClientIdAsync(clientId);
        var allScopes = await _scopeRepository.GetAllAsync();

        var expanded = ExpandHierarchy(assignedScopes, allScopes, s => s.Id, s => s.ParentId, s => s.Name);

        await _cache.SetScopesAsync(clientId, expanded);
        return Result.Success<IList<string>>(expanded);
    }

    private static List<string> ExpandHierarchy<T>(IEnumerable<T> assigned, IEnumerable<T> all, Func<T, Guid> idSelector, Func<T, Guid?> parentIdSelector, Func<T, string> nameSelector)
    {
        var allList = all.ToList();
        var result = new HashSet<string>();

        foreach (var item in assigned)
        {
            AddDescendants(item, allList, result, idSelector, parentIdSelector, nameSelector);
        }

        return result.ToList();
    }

    private static void AddDescendants<T>(T item, List<T> all, HashSet<string> result, Func<T, Guid> idSelector, Func<T, Guid?> parentIdSelector, Func<T, string> nameSelector)
    {
        var name = nameSelector(item);
        if (result.Contains(name)) return;
        result.Add(name);

        var id = idSelector(item);
        var children = all.Where(x => parentIdSelector(x) == id);
        foreach (var child in children)
        {
            AddDescendants(child, all, result, idSelector, parentIdSelector, nameSelector);
        }
    }
}
