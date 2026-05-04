using Auth.Api.Extensions;
using Auth.Api.Middleware;
using Auth.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ──── Auth Module ────
builder.Services.AddAuthModule(builder.Configuration);

// ──── Swagger ────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Auth Module API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// ──── Swagger (Dev only) ────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ──── Middleware Pipeline ────
// Order matters: Authentication → TenantResolution → ClaimsEnrichment → Authorization
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<ClaimsEnrichmentMiddleware>();
app.UseAuthorization();

app.MapControllers();

// ──── Seed Database ────
await DbSeeder.SeedAsync(app.Services);

app.Run();
