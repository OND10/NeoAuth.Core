using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
    {
        var result = await _userService.GetAllAsync(filter);
        return Ok(result.Data);
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var result = await _userService.GetProfileAsync(userId);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(result.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "User updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "User deleted successfully." });
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _userService.ToggleUserStatusAsync(id, true);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "User activated successfully." });
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _userService.ToggleUserStatusAsync(id, false);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "User deactivated successfully." });
    }
}
