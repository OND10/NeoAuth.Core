using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // 1. Seed Permissions
        if (!await context.Permissions.AnyAsync())
        {
            var read = new Permission { Name = "Products.Read", Description = "Can view products" };
            var create = new Permission { Name = "Products.Create", Description = "Can create products" };
            var delete = new Permission { Name = "Products.Delete", Description = "Can delete products" };
            
            context.Permissions.AddRange(read, create, delete);
            await context.SaveChangesAsync();

            // Create Parent Permission
            var manager = new Permission 
            { 
                Name = "Products.Manager", 
                Description = "Full product management",
                ParentId = null // Root
            };
            context.Permissions.Add(manager);
            await context.SaveChangesAsync();

            // Assign children
            read.ParentId = manager.Id;
            create.ParentId = manager.Id;
            delete.ParentId = manager.Id;
            
            context.Permissions.UpdateRange(read, create, delete);
            await context.SaveChangesAsync();
        }

        // 2. Seed Roles
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            adminRole = new ApplicationRole { Name = "Admin", Description = "Administrator role" };
            await roleManager.CreateAsync(adminRole);
            
            // Assign only root permissions to Admin (Expansion will handle children)
            var rootPermissions = await context.Permissions.Where(p => p.ParentId == null).ToListAsync();
            foreach (var p in rootPermissions)
            {
                context.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = p.Id });
            }
        }

        var userRole = await roleManager.FindByNameAsync("User");
        if (userRole == null)
        {
            userRole = new ApplicationRole { Name = "User", Description = "Standard user role" };
            await roleManager.CreateAsync(userRole);
            
            // Assign Read permission to User
            var readPerm = await context.Permissions.FirstAsync(x => x.Name == "Products.Read");
            context.RolePermissions.Add(new RolePermission { RoleId = userRole.Id, PermissionId = readPerm.Id });
        }
        await context.SaveChangesAsync();

        // 3. Seed Users
        if (await userManager.FindByEmailAsync("admin@auth.com") == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@auth.com",
                Email = "admin@auth.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (await userManager.FindByEmailAsync("user@auth.com") == null)
        {
            var regularUser = new ApplicationUser
            {
                UserName = "user@auth.com",
                Email = "user@auth.com",
                FirstName = "Regular",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(regularUser, "User123!");
            await userManager.AddToRoleAsync(regularUser, "User");
        }

        // 4. Seed Scopes
        if (!await context.Scopes.AnyAsync())
        {
            var read = new Scope { Name = "products.read", Description = "Read access" };
            var write = new Scope { Name = "products.write", Description = "Write access" };
            var delete = new Scope { Name = "products.delete", Description = "Delete access" };

            context.Scopes.AddRange(read, write, delete);
            await context.SaveChangesAsync();

            // Create Parent Scope
            var fullAccess = new Scope 
            { 
                Name = "products.full_access", 
                Description = "Full access to products",
                ParentId = null 
            };
            context.Scopes.Add(fullAccess);
            await context.SaveChangesAsync();

            // Assign children
            read.ParentId = fullAccess.Id;
            write.ParentId = fullAccess.Id;
            delete.ParentId = fullAccess.Id;

            context.Scopes.UpdateRange(read, write, delete);
            await context.SaveChangesAsync();
        }

        // 5. Seed Client Application
        if (!await context.ClientApplications.AnyAsync(x => x.ClientId == "ProductManagerClient"))
        {
            var secret = "ClientSecret123!";
            var secretHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
            
            var client = new ClientApplication
            {
                ClientId = "ProductManagerClient",
                ClientSecretHash = secretHash,
                Name = "Product Manager M2M Client",
                Description = "A client for managing products via API",
                IsActive = true
            };
            
            context.ClientApplications.Add(client);
            await context.SaveChangesAsync();

            // Assign Parent Scope to Client (This should enrich all children)
            var fullAccessScope = await context.Scopes.FirstAsync(x => x.Name == "products.full_access");
            context.ClientScopes.Add(new ClientScope { ClientApplicationId = client.Id, ScopeId = fullAccessScope.Id });
            await context.SaveChangesAsync();
        }
    }
}
