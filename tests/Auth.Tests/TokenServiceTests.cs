using Auth.Application.Configuration;
using Auth.Application.Services;
using Auth.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Auth.Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly IOptions<AuthOptions> _options;

    public TokenServiceTests()
    {
        _options = Options.Create(new AuthOptions
        {
            JwtSecret = "super-secret-key-at-least-32-chars-long",
            JwtIssuer = "test-issuer",
            JwtAudience = "test-audience",
            AccessTokenExpirationMinutes = 60,
            UseEnrichedTokens = true
        });

        _tokenService = new TokenService(_options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var roles = new List<string> { "Admin" };
        var permissions = new List<string> { "Products.Read" };

        // Act
        var token = _tokenService.GenerateAccessToken(user, roles, null, permissions);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("test-issuer");
        jwtToken.Audiences.Should().Contain("test-audience");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == "permission" && c.Value == "Products.Read");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueString()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }
}
