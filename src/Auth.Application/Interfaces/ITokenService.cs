using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, Guid? tenantId, IList<string>? permissions = null);
    string GenerateClientAccessToken(ClientApplication client, IEnumerable<string> scopes);
    string GenerateReferenceToken();
    string GenerateRefreshToken(string prefix = "");
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>Generates a short-lived temporary token used only for device verification.</summary>
    string GenerateTemporaryDeviceVerificationToken(ApplicationUser user, Guid deviceId);
    
    /// <summary>Validates a temporary device verification token and returns the principal.</summary>
    ClaimsPrincipal? ValidateTemporaryDeviceVerificationToken(string token);
}
