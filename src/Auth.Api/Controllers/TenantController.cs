using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _tenantService.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _tenantService.GetByIdAsync(id);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        var result = await _tenantService.CreateAsync(request);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPost("{tenantId:guid}/users")]
    public async Task<IActionResult> AssignUser(Guid tenantId, [FromBody] AssignUserToTenantRequest request)
    {
        var result = await _tenantService.AssignUserAsync(tenantId, request);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "User assigned to tenant." });
    }

    [HttpDelete("{tenantId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid tenantId, Guid userId)
    {
        var result = await _tenantService.RemoveUserAsync(tenantId, userId);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "User removed from tenant." });
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserTenants(Guid userId)
    {
        var result = await _tenantService.GetUserTenantsAsync(userId);
        return Ok(result.Value);
    }
}
