using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, Guid? tenantId, IList<string>? permissions = null);
    string GenerateClientAccessToken(ClientApplication client, IEnumerable<string> scopes);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
