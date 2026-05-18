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
}
