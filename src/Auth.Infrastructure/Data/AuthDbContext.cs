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
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}

/// <summary>
/// Provides the current tenant context (set by middleware).
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
}
