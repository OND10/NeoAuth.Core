using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>List all active devices for the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyDevices()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await _deviceService.GetUserDevicesAsync(userId);
        return result.IsFailure ? BadRequest(result) : Ok(result);
    }

    /// <summary>Revoke one of the current user's devices.</summary>
    [HttpDelete("{deviceId:guid}")]
    public async Task<IActionResult> RevokeDevice(Guid deviceId)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await _deviceService.RevokeDeviceAsync(userId, deviceId);
        return result.IsFailure ? BadRequest(result) : Ok(result);
    }

    /// <summary>Rename a device's friendly name.</summary>
    [HttpPatch("{deviceId:guid}/name")]
    public async Task<IActionResult> RenameDevice(Guid deviceId, [FromBody] RenameDeviceRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var result = await _deviceService.RenameDeviceAsync(userId, deviceId, request.FriendlyName);
        return result.IsFailure ? BadRequest(result) : Ok(result);
    }

    /// <summary>Admin: view a specific user's devices.</summary>
    [HttpGet("admin/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserDevices(Guid userId)
    {
        var result = await _deviceService.GetUserDevicesAsync(userId);
        return result.IsFailure ? BadRequest(result) : Ok(result);
    }

    /// <summary>Admin: force-revoke any device.</summary>
    [HttpDelete("admin/{deviceId:guid}/force")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminForceRevoke(Guid deviceId)
    {
        var result = await _deviceService.AdminRevokeDeviceAsync(deviceId);
        return result.IsFailure ? BadRequest(result) : Ok(result);
    }
}
