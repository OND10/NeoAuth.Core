using System.Runtime.CompilerServices;

namespace Auth.Domain.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    // Authentication Errors
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid email or password.");
    public static readonly Error UserNotFound = new("Auth.UserNotFound", "User not found.");
    public static readonly Error UserLocked = new("Auth.UserLocked", "Account is locked. Try again later.");
    public static readonly Error UserInactive = new("Auth.UserInactive", "Account is inactive.");
    public static readonly Error EmailNotConfirmed = new("Auth.EmailNotConfirmed", "Email is not confirmed.");
    public static readonly Error InvalidToken = new("Auth.InvalidToken", "The provided token is invalid.");
    public static readonly Error TokenExpired = new("Auth.TokenExpired", "The token has expired.");
    public static readonly Error RefreshTokenExpired = new("Auth.RefreshTokenExpired", "The refresh token has expired.");
    public static readonly Error RefreshTokenRevoked = new("Auth.RefreshTokenRevoked", "The refresh token has been revoked.");

    // Registration Errors
    public static readonly Error EmailAlreadyExists = new("Auth.EmailAlreadyExists", "A user with this email already exists.");
    public static readonly Error RegistrationFailed = new("Auth.RegistrationFailed", "User registration failed.");

    // Client Errors
    public static readonly Error ClientNotFound = new("Client.NotFound", "Client application not found.");
    public static readonly Error ClientInactive = new("Client.Inactive", "Client application is inactive.");
    public static readonly Error InvalidClientSecret = new("Client.InvalidSecret", "Invalid client secret.");

    // Tenant Errors
    public static readonly Error TenantNotFound = new("Tenant.NotFound", "Tenant not found.");
    public static readonly Error TenantInactive = new("Tenant.Inactive", "Tenant is inactive.");
    public static readonly Error UserNotInTenant = new("Tenant.UserNotInTenant", "User does not belong to this tenant.");

    // Role & Permission Errors
    public static readonly Error RoleNotFound = new("Role.NotFound", "Role not found.");
    public static readonly Error PermissionNotFound = new("Permission.NotFound", "Permission not found.");
    public static readonly Error RoleAlreadyExists = new("Role.AlreadyExists", "A role with this name already exists.");
    public static readonly Error PermissionAlreadyAssigned = new("Permission.AlreadyAssigned", "Permission is already assigned.");
    public static readonly Error ScopeNotFound = new("Scope.NotFound", "Scope not found.");

    // General
    public static readonly Error InternalError = new("General.InternalError", "An unexpected error occurred.");
    public static readonly Error UserEmailConfirmationFailed = new("User.EmailConfirmationFailed", "Email confirmation failed.");
    
    public static Error NotFound(string code, string message)
    {
        return new(code, message);
    }

    public static Error Validation(string code,  string message)
    {
        return new(code, message);
    }
}
