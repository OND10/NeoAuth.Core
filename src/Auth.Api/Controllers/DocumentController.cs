using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ITenantContext _tenantContext;

    public DocumentController(IDocumentService documentService, ITenantContext tenantContext)
    {
        _documentService = documentService;
        _tenantContext = tenantContext;
    }

    #region User Endpoints

    [HttpGet("required")]
    public async Task<IActionResult> GetRequiredDocuments()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var result = await _documentService.GetRequiredDocumentsAsync(roles);
        return Ok(result);
    }

    [HttpPost("bulk-upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitBulkVerification([FromForm] BulkVerificationRequest request)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var tenantId = _tenantContext.TenantId;
        //if (tenantId == Guid.Empty) return BadRequest("Tenant context is missing.");

        var result = await _documentService.SubmitBulkVerificationAsync(userId, tenantId ?? Guid.Empty, request);
        return result.IsFailure ? BadRequest(result) : Ok(result);
    }

    [HttpGet("my-status")]
    public async Task<IActionResult> GetMyStatus()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty) return BadRequest("Tenant context is missing.");

        var result = await _documentService.GetUserStatusAsync(userId, tenantId ?? Guid.Empty);
        if (result.IsFailure) return NotFound(result);
        return Ok(result);
    }

    #endregion

    #region Admin Endpoints (Verification Workflow)

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty) return BadRequest("Tenant context is missing.");

        var result = await _documentService.GetPendingRequestsAsync();
        return Ok(result);
    }

    [HttpPost("review/{requestId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReviewRequest(Guid requestId, [FromBody] ReviewVerificationRequest review)
    {
        var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdString, out var adminId)) return Unauthorized();

        var result = await _documentService.ReviewRequestAsync(requestId, review, adminId);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }

    #endregion

    #region Admin Endpoints (Document Management)

    [HttpGet("required-docs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRequiredDocuments()
    {
        var result = await _documentService.GetTenantRequiredDocumentsAsync();
        return Ok(result);
    }

    [HttpPost("required-docs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRequiredDocument([FromBody] ConfigureRequiredDocumentRequest request)
    {
        var result = await _documentService.CreateRequiredDocumentAsync(request);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("required-docs/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRequiredDocument(Guid id, [FromBody] ConfigureRequiredDocumentRequest request)
    {
        var result = await _documentService.UpdateRequiredDocumentAsync(id, request);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("required-docs/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRequiredDocument(Guid id)
    {
        var result = await _documentService.DeleteRequiredDocumentAsync(id);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }

    #endregion
}
