using Auth.Domain.Common;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Abstracts user management operations (wraps ASP.NET Identity UserManager/SignInManager).
/// Implementation lives in Infrastructure layer.
/// </summary>
public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser?> FindByIdAsync(Guid id);
    Task<PagedResult<ApplicationUser>> GetAllAsync(PaginationFilter filter);

    /// <summary>
    /// Creates a new user with password. Returns (success, errors).
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> CreateAsync(ApplicationUser user, string password);

    /// <summary>
    /// Creates a new user without password (e.g., OAuth). Returns (success, errors).
    /// </summary>
    Task<(bool Succeeded, string[] Errors)> CreateAsync(ApplicationUser user);

    Task UpdateAsync(ApplicationUser user);
    Task<(bool Succeeded, string[] Errors)> DeleteAsync(ApplicationUser user);

    /// <summary>
    /// Validates password and checks lockout. Returns result enum.
    /// </summary>
    Task<SignInResultType> CheckPasswordAsync(ApplicationUser user, string password);

    Task<IList<string>> GetRolesAsync(ApplicationUser user);

    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code);
    Task<string> GenerateUserEmailConfirmationTokenAsync(ApplicationUser user);
    Task<ApplicationUser?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<bool> UpdateLastActiveAsync(Guid userId);
}

public enum SignInResultType
{
    Success,
    Failed,
    LockedOut
}
