using System.ComponentModel.DataAnnotations;
using Auth.Domain.Common;

namespace Auth.Application.DTOs;

// ──────────────────────────── Auth DTOs ────────────────────────────

public record LoginRequest(
    [Required] string Email,
    [Required] string Password,
    Guid? TenantId = null
);

public record RegisterRequest(
    [Required] string Email,
    [Required] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    Guid? TenantId = null
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer"
);

public record RefreshTokenRequest(
    [Required] string RefreshToken
);

public record ForgotPasswordRequest(
    [Required][EmailAddress] string Email
);

public record ResetPasswordRequest(
    [Required][EmailAddress] string Email,
    [Required] string Token,
    [Required][MinLength(6)] string NewPassword
);

public record GoogleLoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string FirstName,
    [Required] string LastName
);

// ──────────────────────────── Client DTOs ────────────────────────────

public record ClientCredentialsRequest(
    [Required] string ClientId,
    [Required] string ClientSecret
);

public record ClientCredentialsResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType = "Bearer"
);

public record CreateClientRequest(
    [Required] string Name,
    string? Description,
    List<Guid> ScopeIds
);

public record CreateClientResponse(
    string ClientId,
    string ClientSecret,
    string Name,
    List<Guid> ScopeIds
);

public record RotateSecretResponse(
    string NewSecret
);

public record UpdateClientScopesRequest(
    [Required] List<Guid> ScopeIds
);

// ──────────────────────────── Scope DTOs ────────────────────────────

public record CreateScopeRequest(
    [Required] string Name,
    string? Description
);

public record UpdateScopeRequest(
    [Required] string Name,
    string? Description
);

public record ScopeResponse(
    Guid Id,
    string Name,
    string? Description
);

// ──────────────────────────── Role & Permission DTOs ────────────────────────────

public record CreateRoleRequest(
    [Required] string Name,
    string? Description,
    Guid? TenantId = null
);

public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? TenantId,
    List<string> Permissions
);

public record AssignPermissionRequest(
    [Required] Guid PermissionId
);

public record AssignPermissionsBulkRequest(
    [Required] List<Guid> PermissionIds
);

public record CreatePermissionRequest(
    [Required] string Name,
    string? Description,
    Guid? ParentId = null
);

public record PermissionResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentId
);

public record UpdatePermissionRequest(
    [Required] string Name,
    string? Description,
    Guid? ParentId = null
);

// ──────────────────────────── Tenant DTOs ────────────────────────────

public record CreateTenantRequest(
    [Required] string Name,
    string? Subdomain
);

public record TenantResponse(
    Guid Id,
    string Name,
    string? Subdomain,
    bool IsActive
);

public record AssignUserToTenantRequest(
    [Required] Guid UserId,
    bool IsDefault = false
);

// ──────────────────────────── User Management DTOs ────────────────────────────

public record UpdateUserRequest(
    [Required] string FirstName,
    [Required] string LastName
);

public record UserProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    string AuthProvider,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    string AuthProvider,
    string? GoogleId
);

public record AssignUserPermissionRequest(
    [Required] Guid UserId,
    [Required] Guid PermissionId
);

public class AuthTokenDto
{
	/// <summary>
	/// Gets or sets the access token
	/// </summary>
	public string AccessToken { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the refresh token
	/// </summary>
	public string RefreshToken { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the token type (usually "Bearer")
	/// </summary>
	public string TokenType { get; set; } = "Bearer";

	/// <summary>
	/// Gets or sets the expiration time in seconds
	/// </summary>
	public int ExpiresIn { get; set; }

	/// <summary>
	/// Gets or sets the user information
	/// </summary>
	public UserResponse User { get; set; } = null!;
}

public class GoogleAuthRequest
{
	/// <summary>
	/// Gets or sets the Google OAuth access token
	/// </summary>
	public string GoogleToken { get; set; } = string.Empty;
}
