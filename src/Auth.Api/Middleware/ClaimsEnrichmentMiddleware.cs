using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Api.Middleware;

/// <summary>
/// Loads permissions/scopes from DB/cache and attaches them to HttpContext.User.
/// Runs AFTER authentication, BEFORE authorization.
/// 
/// API Gateway compatibility: This middleware reads claims from the JWT that was 
/// already validated by the authentication middleware. Whether the request comes 
/// directly from a client or through an API gateway (YARP/Ocelot), the JWT claims 
/// are the same — the gateway forwards the token and our middleware enriches the 
/// principal. No gateway-specific logic is needed.
/// </summary>
public class ClaimsEnrichmentMiddleware
{
    private readonly RequestDelegate _next;

    public ClaimsEnrichmentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IClaimsService claimsService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tokenType = context.User.FindFirst("token_type")?.Value;

            if (tokenType == "user")
            {
                await EnrichUserClaimsAsync(context, claimsService);
            }
            else if (tokenType == "client")
            {
                await EnrichClientClaimsAsync(context, claimsService);
            }
        }

        await _next(context);
    }

    private static async Task EnrichUserClaimsAsync(HttpContext context, IClaimsService claimsService)
    {
        var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return;

        // Check if permissions are already in the token (enriched mode)
        if (context.User.HasClaim(c => c.Type == "permission"))
            return;

        var tenantClaim = context.User.FindFirst("tenant_id");
        Guid? tenantId = tenantClaim is not null && Guid.TryParse(tenantClaim.Value, out var tid) ? tid : null;

        var permissionsResult = await claimsService.GetUserPermissionsAsync(userGuid, tenantId);

        if (permissionsResult.IsSuccess && permissionsResult.Data.Any())
        {
            var identity = new ClaimsIdentity("PermissionsEnrichment");
            foreach (var permission in permissionsResult.Data)
            {
                identity.AddClaim(new Claim("permission", permission));
            }
            context.User.AddIdentity(identity);
        }
    }

    private static async Task EnrichClientClaimsAsync(HttpContext context, IClaimsService claimsService)
    {
        // Client tokens use "client_id" claim (not "sub") — this is a string like "client_abc123"
        var clientId = context.User.FindFirst("client_id")?.Value;

        if (string.IsNullOrEmpty(clientId))
            return;

        // Check if scopes are already in the token (enriched mode)
        if (context.User.HasClaim(c => c.Type == "scope"))
            return;

        var scopesResult = await claimsService.GetClientScopesAsync(clientId);

        if (scopesResult.IsSuccess && scopesResult.Data.Any())
        {
            var identity = new ClaimsIdentity("ScopesEnrichment");
            foreach (var scope in scopesResult.Data)
            {
                identity.AddClaim(new Claim("scope", scope));
            }
            context.User.AddIdentity(identity);
        }
    }
}
