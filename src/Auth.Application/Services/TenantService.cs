using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;

    public TenantService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantResponse>> CreateAsync(CreateTenantRequest request)
    {
        var tenant = new Tenant { Name = request.Name, Subdomain = request.Subdomain };
        await _tenantRepository.AddAsync(tenant);
        return Result.Success(new TenantResponse(tenant.Id, tenant.Name, tenant.Subdomain, tenant.IsActive));
    }

    public async Task<Result<IEnumerable<TenantResponse>>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return Result.Success(tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Subdomain, t.IsActive)));
    }

    public async Task<Result<TenantResponse>> GetByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant is null) return Result.Failure<TenantResponse>(Error.TenantNotFound);
        return Result.Success(new TenantResponse(tenant.Id, tenant.Name, tenant.Subdomain, tenant.IsActive));
    }

    public async Task<Result> AssignUserAsync(Guid tenantId, AssignUserToTenantRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null) return Result.Failure(Error.TenantNotFound);
        await _tenantRepository.AddUserToTenantAsync(new UserTenant { UserId = request.UserId, TenantId = tenantId, IsDefault = request.IsDefault });
        return Result.Success();
    }

    public async Task<Result> RemoveUserAsync(Guid tenantId, Guid userId)
    {
        await _tenantRepository.RemoveUserFromTenantAsync(userId, tenantId);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<TenantResponse>>> GetUserTenantsAsync(Guid userId)
    {
        var tenants = await _tenantRepository.GetTenantsByUserIdAsync(userId);
        return Result.Success(tenants.Select(t => new TenantResponse(t.Id, t.Name, t.Subdomain, t.IsActive)));
    }
}
