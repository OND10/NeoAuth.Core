using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Interfaces;
using Auth.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IEmailService _emailService;
    private readonly AuthOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IClaimsService _claimsService;
    private readonly IClientRepository _clientRepository;


    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IPermissionRepository permissionRepository,
        IEmailService emailService,
        IOptions<AuthOptions> options,
        IHttpClientFactory httpClientFactory,
        IClaimsService claimsService,
        IClientRepository clientRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _permissionRepository = permissionRepository;
        _clientRepository = clientRepository;
        _emailService = emailService;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _claimsService = claimsService;
    }

    public async Task<Result<ClientCredentialsResponse>> AuthenticateClientAsync(ClientCredentialsRequest request)
    {
        var client = await _clientRepository.GetByClientIdAsync(request.ClientId);
        if (client == null)
            return Result.Failure<ClientCredentialsResponse>(Error.ClientNotFound);

        if (!client.IsActive)
            return Result.Failure<ClientCredentialsResponse>(Error.ClientInactive);

        // Simple string comparison for now (in production use hash)
        var inputHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.ClientSecret)));
        if (client.ClientSecretHash != inputHash)
            return Result.Failure<ClientCredentialsResponse>(Error.InvalidClientSecret);

        IList<string>? scopes = null;
        if (_options.UseEnrichedTokens)
        {
            var scopesResult = await _claimsService.GetClientScopesAsync(client.ClientId);
            scopes = scopesResult.Value;
        }
        else
        {
            scopes = client.AllowedScopes.Select(cs => cs.Scope.Name).ToList();
        }

        var token = _tokenService.GenerateClientAccessToken(client, scopes);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ClientApplicationId = client.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays)
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        return Result.Success<ClientCredentialsResponse>(new ClientCredentialsResponse(
            AccessToken: token,
            RefreshToken: refreshTokenValue,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes)), "Client authenticated successfully.");
    }

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<TokenResponse>(Error.InvalidCredentials);

        if (!user.IsActive)
            return Result.Failure<TokenResponse>(Error.UserInactive);

        var signInResult = await _userRepository.CheckPasswordAsync(user, request.Password);

        if (signInResult == SignInResultType.LockedOut)
            return Result.Failure<TokenResponse>(Error.UserLocked);

        if (signInResult != SignInResultType.Success)
            return Result.Failure<TokenResponse>(Error.InvalidCredentials);

        return await GenerateTokensForUserAsync(user, request.TenantId);
    }

    public async Task<Result<ApplicationUser>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result.Failure<ApplicationUser>(Error.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            AuthProvider = AuthProvider.Local,
        };

        var (succeeded, errors) = await _userRepository.CreateAsync(user, request.Password);
        if (!succeeded)
        {
            var errorMessage = string.Join("; ", errors);
            return Result.Failure<ApplicationUser>(new Error("Auth.RegistrationFailed", errorMessage));
        }

        return Result.Success<ApplicationUser>(user, "User registered successfully.");
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (storedToken is null)
            return Result.Failure<TokenResponse>(Error.InvalidToken);

        if (storedToken.IsRevoked)
            return Result.Failure<TokenResponse>(Error.RefreshTokenRevoked);

        if (storedToken.IsExpired)
            return Result.Failure<TokenResponse>(Error.RefreshTokenExpired);

        // Revoke the old token (rotation)
        storedToken.RevokedAt = DateTime.UtcNow;

        var user = storedToken.User;
        if (user is null || !user.IsActive)
            return Result.Failure<TokenResponse>(Error.UserInactive);

        // Determine tenant from the old token context
        var tenantId = user.UserTenants.FirstOrDefault(ut => ut.IsDefault)?.TenantId;

        storedToken.ReplacedByToken = _tokenService.GenerateRefreshToken();
        await _refreshTokenRepository.UpdateAsync(storedToken);

        return await GenerateTokensForUserAsync(user, tenantId);
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Success("User not found"); // Don't reveal if user exists

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email!, token);

        return Result.Success("Password reset link sent successfully.");
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure(Error.UserNotFound);

        var (succeeded, errors) = await _userRepository.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!succeeded)
        {
            var errorMessage = string.Join("; ", errors);
            return Result.Failure(new Error("Auth.ResetFailed", errorMessage));
        }

        // Revoke all refresh tokens on password reset
        await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);

        return Result.Success("Password reset successfully.");
    }

    public async Task<Result> RevokeRefreshTokenAsync(string token)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (storedToken is null)
            return Result.Failure(Error.InvalidToken);

        storedToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        return Result.Success("Token is revoked successfully");
    }

    public async Task<Result<TokenResponse>> GoogleLoginAsync(string email, string firstName, string lastName)
    {
        var user = await _userRepository.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                AuthProvider = AuthProvider.Google
            };

            var (succeeded, _) = await _userRepository.CreateAsync(user);
            if (!succeeded)
                return Result.Failure<TokenResponse>(Error.RegistrationFailed);
        }

        if (!user.IsActive)
            return Result.Failure<TokenResponse>(Error.UserInactive);

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return await GenerateTokensForUserAsync(user, null);
    }

    // ──── Private Helpers ────

    private async Task<Result<TokenResponse>> GenerateTokensForUserAsync(ApplicationUser user, Guid? tenantId)
    {
        var roles = await _userRepository.GetRolesAsync(user);

        IList<string>? permissions = null;
        if (_options.UseEnrichedTokens)
        {
            var permissionsResult = await _claimsService.GetUserPermissionsAsync(user.Id, tenantId);
            permissions = permissionsResult.Value;
        }

        var accessToken = _tokenService.GenerateAccessToken(user, roles, tenantId, permissions);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays)
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return Result.Success(new TokenResponse(
            AccessToken: accessToken,
            RefreshToken: refreshTokenValue,
            ExpiresAt: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes)
        ), "User authenticated successfully.");
    }

    public async Task<Result<AuthTokenDto>> AuthenticateWithGoogleAsync(string googleToken, CancellationToken cancellationToken = default)
{
	// Verify Google token and get user info
	var googleUser = await VerifyGoogleTokenAsync(googleToken, cancellationToken);

	// Find or create user
	var user = await GetOrCreateUserAsync(googleUser.Value, cancellationToken);

	// Update last active time
	await _userRepository.UpdateLastActiveAsync(user.Id);

	// Generate tokens
	var tokenResult = await GenerateTokensForUserAsync(user, null);
	if (tokenResult.IsFailure) return Result.Failure<AuthTokenDto>(tokenResult.Error);

	return new AuthTokenDto
	{
		AccessToken = tokenResult.Value.AccessToken,
		RefreshToken = tokenResult.Value.RefreshToken,
		TokenType = "Bearer",
		ExpiresIn = _options.AccessTokenExpirationMinutes,
		User = googleUser.Value
	};
}


/// <summary>
/// Gets an existing user or creates a new one from Google user information
/// </summary>
/// <param name="googleUser">The Google user information</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>The user entity</returns>
private async Task<ApplicationUser> GetOrCreateUserAsync(UserResponse googleUser, CancellationToken cancellationToken)
{
	// Try to find user by Google ID first
	var user = await _userRepository.GetByGoogleIdAsync(googleUser.Id.ToString(), cancellationToken);

	if (user == null)
	{
		// Try to find by email
		user = await _userRepository.FindByEmailAsync(googleUser.Email);

		if (user != null)
		{
			// Update existing user with Google ID
			user.GoogleId = googleUser.GoogleId;
			user.FirstName = googleUser.FirstName ?? user.FirstName;
			await _userRepository.UpdateAsync(user);
		}
		else
		{
			// Create new user
			user = new ApplicationUser
			{
				Email = googleUser.Email,
				UserName = googleUser.Email,
				FirstName = googleUser.FirstName ?? string.Empty,
				LastName = googleUser.LastName ?? string.Empty,
				GoogleId = googleUser.GoogleId ?? string.Empty,
				AuthProvider = AuthProvider.Google
			};

			await _userRepository.CreateAsync(user);
		}
	}

	return user;
}

    /// <summary>
    /// Verifies a Google OAuth token and returns user information
    /// </summary>
    /// <param name="googleToken">The Google OAuth token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Google user information</returns>
    private async Task<Result<UserResponse>> VerifyGoogleTokenAsync(string googleToken, CancellationToken cancellationToken)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={googleToken}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var googleUser = JsonSerializer.Deserialize<UserResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
            {
                throw new UnauthorizedAccessException("Invalid Google token.");
            }

            return googleUser;
        }
        catch (HttpRequestException ex)
        {
            throw new UnauthorizedAccessException("Failed to verify Google token.", ex);
        }
    }
}
