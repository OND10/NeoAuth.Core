using Auth.Application.DTOs;
using Auth.Application.Services;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Auth.Tests;

public class DeviceServiceTests
{
    private readonly Mock<IDeviceRepository> _deviceRepoMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly DeviceService _deviceService;

    public DeviceServiceTests()
    {
        _deviceRepoMock = new Mock<IDeviceRepository>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _tenantRepoMock = new Mock<ITenantRepository>();
        
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

        _deviceService = new DeviceService(
            _deviceRepoMock.Object,
            _refreshRepoMock.Object,
            _tenantRepoMock.Object,
            _userManagerMock.Object);
    }

    [Fact]
    public async Task RegisterOrUpdateDeviceAsync_NewDevice_ShouldReturnPendingVerificationStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceInfo = new DeviceInfo("test-fingerprint", "Test Device", "Windows", "Desktop");
        
        _deviceRepoMock.Setup(x => x.GetByFingerprintAsync(userId, It.IsAny<string>()))
            .ReturnsAsync((UserDevice?)null);

        _deviceRepoMock.Setup(x => x.GetActiveDeviceCountAsync(userId))
            .ReturnsAsync(0);

        _deviceRepoMock.Setup(x => x.AddAsync(It.IsAny<UserDevice>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _deviceService.RegisterOrUpdateDeviceAsync(userId, deviceInfo, "127.0.0.1", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(DeviceStatus.PendingVerification);
        result.Value.DeviceFingerprint.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ValidateDeviceAsync_ActiveDevice_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var device = new UserDevice { Status = DeviceStatus.Active };
        
        _deviceRepoMock.Setup(x => x.GetByFingerprintAsync(userId, It.IsAny<string>()))
            .ReturnsAsync(device);

        // Act
        var result = await _deviceService.ValidateDeviceAsync(userId, "test-fingerprint");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(DeviceStatus.Active);
    }
}
