using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);
    Task<Result<TokenResponse>> VerifyDeviceAsync(VerifyDeviceRequest request);
    Task<Result<ApplicationUser>> RegisterAsync(RegisterRequest request);
    Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    Task<Result> RevokeRefreshTokenAsync(string token);
    Task<Result<TokenResponse>> GoogleLoginAsync(string email, string firstName, string lastName);
    Task<Result<AuthTokenDto>> AuthenticateWithGoogleAsync(string googleToken, CancellationToken cancellationToken = default);
    Task<Result<ClientCredentialsResponse>> AuthenticateClientAsync(ClientCredentialsRequest request);
    Task<Result<IntrospectionResponse>> IntrospectAsync(IntrospectionRequest request);
}
