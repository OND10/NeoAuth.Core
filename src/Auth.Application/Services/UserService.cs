using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserResponse>>> GetAllAsync(PaginationFilter filter)
    {
        var pagedUsers = await _userRepository.GetAllAsync(filter);
        var responseItems = pagedUsers.Items.Select(u => new UserResponse(
            u.Id,
            u.Email ?? string.Empty,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.AuthProvider.ToString(),
            u.GoogleId
        ));

        var response = new PagedResult<UserResponse>(responseItems, pagedUsers.TotalCount, pagedUsers.PageNumber, pagedUsers.PageSize);
        return Result.Success(response, "users retrieved successfully");
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<UserProfileResponse>(Error.UserNotFound);

        return Result.Success(new UserProfileResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.AuthProvider.ToString(),
            user.CreatedAt,
            user.LastLoginAt
        ), "user profile retrieved successfully");
    }

    public async Task<Result> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
            return Result.Failure(Error.UserNotFound);

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        await _userRepository.UpdateAsync(user);

        return Result.Success("user updated successfully");
    }

    public async Task<Result> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
            return Result.Failure(Error.UserNotFound);

        var (succeeded, errors) = await _userRepository.DeleteAsync(user);
        if (!succeeded)
            return Result.Failure(new Error("User.DeleteFailed", string.Join(", ", errors)));

        return Result.Success("user deleted successfully");
    }

    public async Task<Result> ToggleUserStatusAsync(Guid id, bool isActive)
    {
        var user = await _userRepository.FindByIdAsync(id);
        if (user is null)
            return Result.Failure(Error.UserNotFound);

        user.IsActive = isActive;
        await _userRepository.UpdateAsync(user);

        return Result.Success("user status toggled successfully");
    }

    public async Task<Result<ApplicationUser>> FindUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        if(user is null)
            return Result.Failure<ApplicationUser>(Error.UserNotFound);

        return Result.Success(user, "user found successfully");
    }

    public async Task<Result<bool>> ConfirmUserEmailAsync(ApplicationUser user, string code)
    {
        var result = await _userRepository.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
            return Result.Failure<bool>(Error.UserEmailConfirmationFailed);

        return Result.Success(true, "user email confirmed successfully");
    }

    public async Task<Result<string>> GenerateUserEmailConfirmationTokenAsync(ApplicationUser user)
    {
        var token = await _userRepository.GenerateUserEmailConfirmationTokenAsync(user);
        if (token is null)
            return Result.Failure<string>(Error.UserEmailConfirmationFailed);

        return Result.Success(token, "user email confirmation token generated successfully");
    }
}
