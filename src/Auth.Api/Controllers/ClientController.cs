using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientAuthService _clientAuthService;

    public ClientController(IClientAuthService clientAuthService)
    {
        _clientAuthService = clientAuthService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
    {
        var result = await _clientAuthService.GetAllAsync(filter);
        return Ok(result);
    }



    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] ClientCredentialsRequest request)
    {
        var result = await _clientAuthService.AuthenticateAsync(request);
        if (result.IsFailure) return Unauthorized(new { result.Error!.Code, result.Error.Message });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        var result = await _clientAuthService.CreateClientAsync(request);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return CreatedAtAction(nameof(Create), result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var result = await _clientAuthService.RevokeClientAsync(id);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(result);
    }

    [HttpPost("{id:guid}/rotate-secret")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RotateSecret(Guid id)
    {
        var result = await _clientAuthService.RotateSecretAsync(id);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(result);
    }

    [HttpPut("{id:guid}/scopes")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateScopes(Guid id, [FromBody] UpdateClientScopesRequest request)
    {
        var result = await _clientAuthService.UpdateScopesAsync(id, request.ScopeIds);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(result);
    }
}
