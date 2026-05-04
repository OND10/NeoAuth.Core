using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// Implements IUserRepository using ASP.NET Identity's UserManager and SignInManager.
/// This is the only place where Identity is referenced for user operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserRepository(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
        => await _userManager.FindByEmailAsync(email);

    public async Task<ApplicationUser?> FindByIdAsync(Guid id)
        => await _userManager.FindByIdAsync(id.ToString());

    public async Task<PagedResult<ApplicationUser>> GetAllAsync(PaginationFilter filter)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(u => u.Email!.Contains(filter.SearchTerm) || u.FirstName.Contains(filter.SearchTerm) || u.LastName.Contains(filter.SearchTerm));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.Email)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<ApplicationUser>(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<(bool Succeeded, string[] Errors)> CreateAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Succeeded, string[] Errors)> CreateAsync(ApplicationUser user)
    {
        var result = await _userManager.CreateAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task UpdateAsync(ApplicationUser user)
        => await _userManager.UpdateAsync(user);

    public async Task<(bool Succeeded, string[] Errors)> DeleteAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<SignInResultType> CheckPasswordAsync(ApplicationUser user, string password)
    {
        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (result.IsLockedOut) return SignInResultType.LockedOut;
        if (result.Succeeded) return SignInResultType.Success;
        return SignInResultType.Failed;
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
        => await _userManager.GetRolesAsync(user);

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        => await _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<(bool Succeeded, string[] Errors)> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string code)
        => await _userManager.ConfirmEmailAsync(user, code);

    public async Task<string> GenerateUserEmailConfirmationTokenAsync(ApplicationUser user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<ApplicationUser?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
	    if (string.IsNullOrWhiteSpace(googleId))
		    return null;
	    
        return await _userManager.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);
    }

    	/// <inheritdoc />
	public async Task<bool> UpdateLastActiveAsync(Guid userId)
	{
		var user = await FindByIdAsync(userId);
		if (user == null)
			return false;

		user.LastLoginAt = DateTime.UtcNow;
		await _userManager.UpdateAsync(user);
		return true;
	}
}
