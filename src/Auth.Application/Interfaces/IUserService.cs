using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IUserService
{
    Task<Result<PagedResult<UserResponse>>> GetAllAsync(PaginationFilter filter);
    Task<Result<UserProfileResponse>> GetProfileAsync(Guid userId);
    Task<Result> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<Result> DeleteUserAsync(Guid id);
    Task<Result> ToggleUserStatusAsync(Guid id, bool isActive);
    Task<Result<ApplicationUser>> FindUserByIdAsync(Guid userId);
    Task<Result<bool>> ConfirmUserEmailAsync(ApplicationUser user, string code);
    Task<Result<string>> GenerateUserEmailConfirmationTokenAsync(ApplicationUser user);
}
