using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Auth.Application.Configuration;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Application.Services;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth.Api.Extensions;

public static class AuthModuleExtensions
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<AuthOptions>? configureOptions = null)
    {
        // Bind configuration
        var authOptions = new AuthOptions();
        configuration.GetSection(AuthOptions.SectionName).Bind(authOptions);
        configureOptions?.Invoke(authOptions);
        services.Configure<AuthOptions>(opt =>
        {
            configuration.GetSection(AuthOptions.SectionName).Bind(opt);
            configureOptions?.Invoke(opt);
        });

        // ──── EF Core ────
        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(authOptions.ConnectionString));

        // ──── Tenant Context (scoped) ────
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // ──── ASP.NET Identity ────
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();

        // ──── JWT Authentication ────
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authOptions.JwtIssuer,
                ValidateAudience = true,
                ValidAudience = authOptions.JwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.JwtSecret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (authHeader != null && authHeader.StartsWith("Bearer ref_", StringComparison.OrdinalIgnoreCase))
                    {
                        // Skip JWT validation for reference tokens
                        context.NoResult();
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // ──── Google OAuth (if configured) ────
        if (!string.IsNullOrEmpty(authOptions.Google.ClientId))
        {
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = authOptions.Google.ClientId;
                    options.ClientSecret = authOptions.Google.ClientSecret;
                });
        }

        // ──── Caching ────
        services.AddMemoryCache();
        services.AddSingleton<IClaimsCacheService, InMemoryClaimsCacheService>();

        // ──── HTTP Client Factory ────
        services.AddHttpClient();

        // ──── Repositories ────
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IReferenceTokenRepository, ReferenceTokenRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IScopeRepository, ScopeRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        

        // ──── Services ────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientAuthService, ClientAuthService>();
        services.AddScoped<IClaimsService, ClaimsService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IScopeService, ScopeService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IFileService, LocalFileService>();
        services.AddScoped<IDeviceService, DeviceService>();

        return services;
    }
}
