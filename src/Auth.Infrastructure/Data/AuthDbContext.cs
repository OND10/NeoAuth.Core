using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Data;

public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly Guid? _currentTenantId;

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public AuthDbContext(DbContextOptions<AuthDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _currentTenantId = tenantContext.TenantId;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ClientApplication> ClientApplications => Set<ClientApplication>();
    public DbSet<Scope> Scopes => Set<Scope>();
    public DbSet<ClientScope> ClientScopes => Set<ClientScope>();
    public DbSet<ReferenceToken> ReferenceTokens => Set<ReferenceToken>();
    public DbSet<RequiredDocument> RequiredDocuments => Set<RequiredDocument>();
    public DbSet<VerificationRequest> VerificationRequests => Set<VerificationRequest>();
    public DbSet<UserDocument> UserDocuments => Set<UserDocument>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<RoleDeviceLimit> RoleDeviceLimits => Set<RoleDeviceLimit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
}
