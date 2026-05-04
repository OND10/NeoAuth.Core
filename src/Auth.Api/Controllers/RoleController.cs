using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionService _permissionService;
    private readonly IPermissionRepository _permissionRepository;

    public RoleController(
        IRoleRepository roleRepository,
        IPermissionService permissionService,
        IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionService = permissionService;
        _permissionRepository = permissionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
    {
        var rolesResult = await _roleRepository.GetAllAsync(filter);
        var response = rolesResult.Items.Select(role => new RoleResponse(
            role.Id, 
            role.Name!, 
            role.Description, 
            role.TenantId, 
            role.RolePermissions.Select(rp => rp.Permission.Name).ToList()
        )).ToList();

        var pagedResponse = new PagedResult<RoleResponse>(response, rolesResult.TotalCount, rolesResult.PageNumber, rolesResult.PageSize);
        return Ok(Result.Success(pagedResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var role = new ApplicationRole { Name = request.Name, Description = request.Description, TenantId = request.TenantId };
        var (succeeded, errors) = await _roleRepository.CreateAsync(role);
        
        if (!succeeded) 
            return BadRequest(Result.Failure(new Error("Role.CreateFailed", string.Join(", ", errors))));
            
        return Ok(Result.Success(new RoleResponse(role.Id, role.Name!, role.Description, role.TenantId, new List<string>())));
    }

    [HttpPost("{roleId:guid}/permissions")]
    public async Task<IActionResult> AssignPermission(Guid roleId, [FromBody] AssignPermissionRequest request)
    {
        var result = await _permissionService.AssignToRoleAsync(roleId, request.PermissionId);
        if (result.IsFailure) return BadRequest(result);
        return Ok(Result.Success("Permission assigned to role."));
    }

    [HttpPost("{roleId:guid}/permissions/bulk")]
    public async Task<IActionResult> AssignPermissionsBulk(Guid roleId, [FromBody] AssignPermissionsBulkRequest request)
    {
        var result = await _permissionService.AssignToRoleBulkAsync(roleId, request.PermissionIds);
        if (result.IsFailure) return BadRequest(result);
        return Ok(Result.Success("Permissions assigned to role successfully."));
    }

    [HttpDelete("{roleId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemovePermission(Guid roleId, Guid permissionId)
    {
        var result = await _permissionService.RemoveFromRoleAsync(roleId, permissionId);
        if (result.IsFailure) return BadRequest(result);
        return Ok(Result.Success("Permission removed from role."));
    }

    [HttpGet("{roleId:guid}/permissions")]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
        var response = permissions.Select(p => new PermissionResponse(p.Id, p.Name, p.Description, p.ParentId));
        return Ok(Result.Success(response));
    }

    // ──── User Permissions ────

    [HttpPost("users/{userId:guid}/permissions")]
    public async Task<IActionResult> AssignUserPermission(Guid userId, [FromBody] AssignPermissionRequest request)
    {
        var result = await _permissionService.AssignToUserAsync(userId, request.PermissionId);
        if (result.IsFailure) return BadRequest(result);
        return Ok(Result.Success("Permission assigned to user."));
    }

    [HttpPost("users/{userId:guid}/permissions/bulk")]
    public async Task<IActionResult> AssignUserPermissionsBulk(Guid userId, [FromBody] AssignPermissionsBulkRequest request)
    {
        var result = await _permissionService.AssignToUserBulkAsync(userId, request.PermissionIds);
        if (result.IsFailure) return BadRequest(result);
        return Ok(Result.Success("Permissions assigned to user successfully."));
    }

    [HttpDelete("users/{userId:guid}/permissions/{permissionId:guid}")]
    public async Task<IActionResult> RemoveUserPermission(Guid userId, Guid permissionId)
    {
        var result = await _permissionService.RemoveFromUserAsync(userId, permissionId);
        if (result.IsFailure) return BadRequest(result);
        return Ok(Result.Success("Permission removed from user."));
    }

    [HttpGet("users/{userId:guid}/permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var permissions = await _permissionRepository.GetPermissionsByUserIdAsync(userId);
        var response = permissions.Select(p => new PermissionResponse(p.Id, p.Name, p.Description, p.ParentId));
        return Ok(Result.Success(response));
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        var roles = await _roleRepository.GetRolesByUserIdAsync(userId);
        var response = roles.Select(role => new RoleResponse(
            role.Id, 
            role.Name!, 
            role.Description, 
            role.TenantId, 
            role.RolePermissions.Select(rp => rp.Permission.Name).ToList()
        ));
        return Ok(Result.Success(response));
    }

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetTenantRoles(Guid tenantId)
    {
        var roles = await _roleRepository.GetRolesByTenantIdAsync(tenantId);
        var response = roles.Select(role => new RoleResponse(
            role.Id, 
            role.Name!, 
            role.Description, 
            role.TenantId, 
            role.RolePermissions.Select(rp => rp.Permission.Name).ToList()
        ));
        return Ok(Result.Success(response));
    }

    [HttpGet("tenant/{tenantId:guid}/permissions")]
    public async Task<IActionResult> GetTenantPermissions(Guid tenantId)
    {
        var roles = await _roleRepository.GetRolesByTenantIdAsync(tenantId);
        var permissions = roles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => new PermissionResponse(rp.Permission.Id, rp.Permission.Name, rp.Permission.Description, rp.Permission.ParentId))
            .DistinctBy(p => p.Id)
            .ToList();
            
        return Ok(Result.Success(permissions));
    }

    // ──── Permissions CRUD ────

    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions([FromQuery] PaginationFilter filter)
    {
        var result = await _permissionService.GetAllAsync(filter);
        return Ok(result);
    }

    [HttpPost("permissions")]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
    {
        var result = await _permissionService.CreateAsync(request);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("permissions/{id:guid}")]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] UpdatePermissionRequest request)
    {
        var result = await _permissionService.UpdateAsync(id, request);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("permissions/{id:guid}")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var result = await _permissionService.DeleteAsync(id);
        if (result.IsFailure) return BadRequest(result);
        return Ok(result);
    }
}
