using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Auth.Api.Authorization;

/// <summary>
/// Requires the user to have a specific permission claim.
/// For user tokens: checks "permission" claim.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tokenType = user.FindFirst("token_type")?.Value;

        // If it's a client token, skip permission check (scope check applies instead)
        if (tokenType == "client")
            return;

        // For user tokens, check permission
        if (!user.HasClaim("permission", _permission))
        {
            context.Result = new ForbidResult();
        }
    }
}

/// <summary>
/// Requires the client to have a specific scope claim.
/// For client (M2M) tokens: checks "scope" claim.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireScopeAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string _scope;

    public RequireScopeAttribute(string scope)
    {
        _scope = scope;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tokenType = user.FindFirst("token_type")?.Value;

        // If it's a user token, skip scope check (permission check applies instead)
        if (tokenType == "user")
            return;

        // For client tokens, check scope
        if (!user.HasClaim("scope", _scope))
        {
            context.Result = new ForbidResult();
        }
    }
}
