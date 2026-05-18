using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Auth.Api.Middleware;

public class ReferenceTokenMiddleware
{
    private readonly RequestDelegate _next;

    public ReferenceTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        string? authHeader = context.Request.Headers["Authorization"];

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ref_", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            var result = await authService.IntrospectAsync(new IntrospectionRequest(token));

            if (result.IsSuccess && result.Value.Active)
            {
                var claims = new List<Claim>
                {
                    new("client_id", result.Value.ClientId ?? string.Empty),
                    new("token_type", "client")
                };

                if (result.Value.Scope != null)
                {
                    foreach (var scope in result.Value.Scope)
                    {
                        claims.Add(new Claim("scope", scope));
                    }
                }

                var identity = new ClaimsIdentity(claims, "ReferenceToken");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}
