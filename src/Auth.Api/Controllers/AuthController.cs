using Auth.Application.Configuration;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;
    private readonly IOptions<AuthOptions> _options;
     

    public AuthController(
        IAuthService authService, 
        IUserService userService, 
        IEmailService emailService,
        ILogger<AuthController> logger,
        IOptions<AuthOptions> options)
    {
        _authService = authService;
        _userService = userService;
        _emailService = emailService;
        _logger = logger;
        _options = options;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Intercept IP Address and User-Agent
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        // Update request object if it was provided
        if (request.Device != null)
        {
            var updatedRequest = request with 
            { 
                IpAddress = ipAddress,
                Device = request.Device with 
                { 
                    // Fallback to User-Agent if Platform/DeviceType not explicitly provided
                    Platform = request.Device.Platform ?? userAgent
                }
            };
            
            var resultWithDevice = await _authService.LoginAsync(updatedRequest);
            if (resultWithDevice.IsFailure) return Unauthorized(resultWithDevice);
            
            // If device requires verification, we return Ok (or a specific status)
            // The client will see RequiresDeviceVerification = true
            return Ok(resultWithDevice);
        }

        var result = await _authService.LoginAsync(request);
        if (result.IsFailure) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("verify-device")]
    public async Task<IActionResult> VerifyDevice([FromBody] VerifyDeviceRequest request)
    {
        var result = await _authService.VerifyDeviceAsync(request);
        if (result.IsFailure) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result.IsFailure) return BadRequest(result);

        var token = await _userService.GenerateUserEmailConfirmationTokenAsync(result.Data);
        var callbackUrl = Url.Action(
                            action: "ConfirmEmail",
                            controller: "Auth",
                            values: new { userId = result.Data.Id, code = token },
                            protocol: "https"
        );

        /// Call email service to send content with link for callback to confirm the email
        /// await _emailService.SendWelcomeEmailAsync(request.Email, $"Please confirm your email by clicking here : <a href='{callbackUrl}'>Link</a>");
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (result.IsFailure) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "Password has been reset successfully." });
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "Token revoked." });
    }

    [HttpGet("google/authorize")]
    public IActionResult GoogleAuthorize()
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl
        };

        return Challenge(properties, "Google");
    }

    /// <summary>
    /// Handles the Google OAuth callback
    /// </summary>
    /// <returns>Redirect to the frontend with authentication result</returns>
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync("Google");
        if (!result.Succeeded)
        {
            _logger.LogWarning("Google authentication failed");
            return Redirect($"{_options.Value.FrontendUrl}?error=auth_failed");
        }

        var googleToken = result.Properties?.GetTokenValue("access_token");
        if (string.IsNullOrEmpty(googleToken))
        {
            _logger.LogWarning("No access token received from Google");
            return Redirect($"{_options.Value.FrontendUrl}?error=no_token");
        }

        var authResult = await _authService.AuthenticateWithGoogleAsync(googleToken);

        // In a real application, you might want to set secure HTTP-only cookies
        // or redirect to a frontend page that handles the tokens
        return Redirect($"{_options.Value.FrontendUrl}?access_token={authResult.Data.AccessToken}&refresh_token={authResult.Data.RefreshToken}");
    }

    /// <summary>
    /// Authenticates with Google using an access token sent by the frontend
    /// </summary>
    /// <param name="request">The authentication request containing the token</param>
    /// <returns>Authentication tokens and user information</returns>
    [HttpPost("google/token")]
    public async Task<ActionResult<AuthTokenDto>> AuthenticateWithGoogle([FromBody] GoogleAuthRequest request)
    {
        if (string.IsNullOrEmpty(request.GoogleToken))
        {
            return BadRequest(new { error = "Google token is required" });
        }

        var result = await _authService.AuthenticateWithGoogleAsync(request.GoogleToken);
        if (result.IsFailure) return Unauthorized(result);
        return Ok(result);
    }


    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string code, [FromQuery] Guid userId)
    {
        if (ModelState.IsValid)
        {
            var user = await _userService.FindUserByIdAsync(userId);
            if (user.IsFailure) return Unauthorized(new { user.Error!.Code, user.Error.Message });

            var result = await _userService.ConfirmUserEmailAsync(user.Data, code);
            if (result.IsSuccess)
                return Ok(new { Message = "Email confirmed successfully." });
        }
        return BadRequest(new { Message = "Email confirmation failed." });
    }

    [HttpPost("client/token")]
    public async Task<IActionResult> AuthenticateClient([FromBody] ClientCredentialsRequest request)
    {
        var result = await _authService.AuthenticateClientAsync(request);
        if (result.IsFailure) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("introspect")]
    public async Task<IActionResult> Introspect([FromBody] IntrospectionRequest request)
    {
        var result = await _authService.IntrospectAsync(request);
        return Ok(result);
    }

}
