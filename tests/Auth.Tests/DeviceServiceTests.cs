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
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly DeviceService _deviceService;

    public DeviceServiceTests()
    {
        _deviceRepoMock = new Mock<IDeviceRepository>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _tenantRepoMock = new Mock<ITenantRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);

        _deviceService = new DeviceService(
            _deviceRepoMock.Object,
            _refreshRepoMock.Object,
            _tenantRepoMock.Object,
            _userManagerMock.Object,
            _roleRepoMock.Object);
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
        result.Data.Status.Should().Be(DeviceStatus.PendingVerification);
        result.Data.DeviceFingerprint.Should().NotBeNullOrEmpty();
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
        result.Data.Status.Should().Be(DeviceStatus.Active);
    }

    [Fact]
    public async Task RegisterOrUpdateDeviceAsync_ExceedsRoleLimit_ShouldEvictOldestDevice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var deviceInfo = new DeviceInfo("test-fingerprint", "Test Device", "Windows", "Desktop");
        
        var userRoles = new List<ApplicationRole> { new() { Id = roleId, Name = "PremiumUser" } };
        _roleRepoMock.Setup(x => x.GetRolesByUserIdAsync(userId)).ReturnsAsync(userRoles);

        var roleLimit = new RoleDeviceLimit { RoleId = roleId, MaxDevices = 2 };
        _deviceRepoMock.Setup(x => x.GetRoleDeviceLimitAsync(roleId)).ReturnsAsync(roleLimit);

        _deviceRepoMock.Setup(x => x.GetByFingerprintAsync(userId, It.IsAny<string>()))
            .ReturnsAsync((UserDevice?)null);

        // We currently have 2 active devices (exceeds limit 2 for new registration)
        _deviceRepoMock.Setup(x => x.GetActiveDeviceCountAsync(userId))
            .ReturnsAsync(2);

        var oldestDevice = new UserDevice { Id = Guid.NewGuid(), UserId = userId, Status = DeviceStatus.Active };
        _deviceRepoMock.Setup(x => x.GetOldestActiveDeviceAsync(userId))
            .ReturnsAsync(oldestDevice);

        _deviceRepoMock.Setup(x => x.UpdateAsync(It.IsAny<UserDevice>()))
            .Returns(Task.CompletedTask);

        _deviceRepoMock.Setup(x => x.AddAsync(It.IsAny<UserDevice>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _deviceService.RegisterOrUpdateDeviceAsync(userId, deviceInfo, "127.0.0.1", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        oldestDevice.Status.Should().Be(DeviceStatus.Revoked);
        _deviceRepoMock.Verify(x => x.UpdateAsync(oldestDevice), Times.Once);
    }

    [Fact]
    public async Task RegisterOrUpdateDeviceAsync_UserOverrideLimit_ShouldPrioritizeUserOverrideOverRoleLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var deviceInfo = new DeviceInfo("test-fingerprint", "Test Device", "Windows", "Desktop");
        
        var user = new ApplicationUser { Id = userId, MaxDevicesLimit = 1 };
        _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        var userRoles = new List<ApplicationRole> { new() { Id = roleId, Name = "PremiumUser" } };
        _roleRepoMock.Setup(x => x.GetRolesByUserIdAsync(userId)).ReturnsAsync(userRoles);

        // Even though role limit is 5, the user override is 1, so the limit should be 1.
        var roleLimit = new RoleDeviceLimit { RoleId = roleId, MaxDevices = 5 };
        _deviceRepoMock.Setup(x => x.GetRoleDeviceLimitAsync(roleId)).ReturnsAsync(roleLimit);

        _deviceRepoMock.Setup(x => x.GetByFingerprintAsync(userId, It.IsAny<string>()))
            .ReturnsAsync((UserDevice?)null);

        // We currently have 1 active device, which meets/exceeds the limit of 1
        _deviceRepoMock.Setup(x => x.GetActiveDeviceCountAsync(userId))
            .ReturnsAsync(1);

        var oldestDevice = new UserDevice { Id = Guid.NewGuid(), UserId = userId, Status = DeviceStatus.Active };
        _deviceRepoMock.Setup(x => x.GetOldestActiveDeviceAsync(userId))
            .ReturnsAsync(oldestDevice);

        _deviceRepoMock.Setup(x => x.UpdateAsync(It.IsAny<UserDevice>()))
            .Returns(Task.CompletedTask);

        _deviceRepoMock.Setup(x => x.AddAsync(It.IsAny<UserDevice>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _deviceService.RegisterOrUpdateDeviceAsync(userId, deviceInfo, "127.0.0.1", null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        oldestDevice.Status.Should().Be(DeviceStatus.Revoked); // Should revoke since user limit is 1, and we had 1 active.
        _deviceRepoMock.Verify(x => x.UpdateAsync(oldestDevice), Times.Once);
    }
}
