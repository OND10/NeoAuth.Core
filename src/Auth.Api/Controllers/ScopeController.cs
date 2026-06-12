using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ScopeController : ControllerBase
{
    private readonly IScopeService _scopeService;

    public ScopeController(IScopeService scopeService)
    {
        _scopeService = scopeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
    {
        var result = await _scopeService.GetAllAsync(filter);
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _scopeService.GetByIdAsync(id);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateScopeRequest request)
    {
        var result = await _scopeService.CreateAsync(request);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScopeRequest request)
    {
        var result = await _scopeService.UpdateAsync(id, request);
        if (result.IsFailure) return BadRequest(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "Scope updated successfully." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _scopeService.DeleteAsync(id);
        if (result.IsFailure) return NotFound(new { result.Error!.Code, result.Error.Message });
        return Ok(new { Message = "Scope deleted." });
    }
}
