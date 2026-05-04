using Auth.Domain.Common;

namespace Auth.Application.Interfaces;

public interface IClaimsService
{
    Task<Result<IList<string>>> GetUserPermissionsAsync(Guid userId, Guid? tenantId);
    Task<Result<IList<string>>> GetClientScopesAsync(string clientId);
}
