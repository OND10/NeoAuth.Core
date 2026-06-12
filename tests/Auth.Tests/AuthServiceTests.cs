using Auth.Application.Configuration;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Auth.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly Mock<IReferenceTokenRepository> _referenceTokenRepoMock;
    private readonly Mock<IPermissionRepository> _permRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<System.Net.Http.IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IClaimsService> _claimsServiceMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _referenceTokenRepoMock = new Mock<IReferenceTokenRepository>();
        _permRepoMock = new Mock<IPermissionRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
        _claimsServiceMock = new Mock<IClaimsService>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        var _deviceServiceMock = new Mock<IDeviceService>();
        
        var options = Microsoft.Extensions.Options.Options.Create(new AuthOptions());

        _authService = new AuthService(
            _userRepoMock.Object,
            _tokenServiceMock.Object,
            _refreshRepoMock.Object,
            _referenceTokenRepoMock.Object,
            _permRepoMock.Object,
            _emailServiceMock.Object,
            options,
            _httpClientFactoryMock.Object,
            _claimsServiceMock.Object,
            _clientRepositoryMock.Object,
            _deviceServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var request = new LoginRequest("test@test.com", "wrong-password");
        _userRepoMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(SignInResultType.Failed);
        _userRepoMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUser());

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.InvalidCredentials);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithClientReferenceToken_ShouldGenerateNewReferenceToken()
    {
        // Arrange
        var clientApp = new ClientApplication
        {
            Id = Guid.NewGuid(),
            ClientId = "client-id",
            IsActive = true
        };

        var storedToken = new RefreshToken
        {
            Token = "ref_client-refresh-token",
            ClientApplicationId = clientApp.Id,
            ClientApplication = clientApp,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _refreshRepoMock.Setup(x => x.GetByTokenAsync("ref_client-refresh-token"))
            .ReturnsAsync(storedToken);

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken("ref_"))
            .Returns("ref_new-client-refresh-token");

        _tokenServiceMock.Setup(x => x.GenerateReferenceToken())
            .Returns("ref_new-client-access-token");

        var request = new RefreshTokenRequest("ref_client-refresh-token");

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.AccessToken.Should().Be("ref_new-client-access-token");
        result.Data.RefreshToken.Should().Be("ref_new-client-refresh-token");
        
        storedToken.RevokedAt.Should().NotBeNull();
        storedToken.ReplacedByToken.Should().Be("ref_new-client-refresh-token");
        _refreshRepoMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => rt.Token == "ref_new-client-refresh-token" && rt.ClientApplicationId == clientApp.Id)), Times.Once);
        _referenceTokenRepoMock.Verify(x => x.AddAsync(It.Is<ReferenceToken>(rt => rt.Token == "ref_new-client-access-token" && rt.ClientId == clientApp.Id)), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithClientJwtToken_ShouldGenerateNewJwtToken()
    {
        // Arrange
        var clientApp = new ClientApplication
        {
            Id = Guid.NewGuid(),
            ClientId = "client-id",
            IsActive = true
        };

        var storedToken = new RefreshToken
        {
            Token = "jwt_client-refresh-token",
            ClientApplicationId = clientApp.Id,
            ClientApplication = clientApp,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _refreshRepoMock.Setup(x => x.GetByTokenAsync("jwt_client-refresh-token"))
            .ReturnsAsync(storedToken);

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken("jwt_"))
            .Returns("jwt_new-client-refresh-token");

        _tokenServiceMock.Setup(x => x.GenerateClientAccessToken(clientApp, It.IsAny<IEnumerable<string>>()))
            .Returns("jwt_new-client-access-token");

        var request = new RefreshTokenRequest("jwt_client-refresh-token");

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.AccessToken.Should().Be("jwt_new-client-access-token");
        result.Data.RefreshToken.Should().Be("jwt_new-client-refresh-token");
        
        storedToken.RevokedAt.Should().NotBeNull();
        storedToken.ReplacedByToken.Should().Be("jwt_new-client-refresh-token");
        _refreshRepoMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => rt.Token == "jwt_new-client-refresh-token" && rt.ClientApplicationId == clientApp.Id)), Times.Once);
        _referenceTokenRepoMock.Verify(x => x.AddAsync(It.IsAny<ReferenceToken>()), Times.Never);
    }
}
