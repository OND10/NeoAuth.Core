# NeoAuth.Core 🛡️

**NeoAuth.Core** is a high-performance, enterprise-grade Authentication and Authorization module for .NET 8. It implements Clean Architecture, Hierarchical Claims, and standard OAuth2/OIDC patterns.

## 🚀 Key Features
- **Hierarchical Security**: Recursive expansion of Permissions and Scopes (e.g., `Product.Manage` -> `product.read`, `product.add`).
- **M2M Support**: Client Credentials flow with full Refresh Token support.
- **Result Pattern**: Consistent API responses using `Result<T>` shape.
- **Thin Token Strategy**: Minimalist JWTs with on-demand runtime enrichment via Middleware.
- **Standardized OAuth2**: Supports login, register, refresh, and introspection patterns.

## 🛠️ Architecture
- **Domain**: Entities, Interfaces, and Common Logic.
- **Application**: DTOs, Services, and Business logic.
- **Infrastructure**: Entity Framework Core, SQL Server, and Cache implementation.
- **Api**: RESTful endpoints and Security Middlewares.

## 🐙 Git & GitHub
The repository is initialized and linked to [NeoAuth.Core](https://github.com/OND10/NeoAuth.Core.git).

### Initial Push
```bash
git add .
git commit -m "feat: core auth module with hierarchical claims"
git branch -M main
git push -u origin main
```
*Note: The Gateway project is ignored by default via .gitignore.*

## 🔒 Opaque Tokens (Planned)
We are planning to implement **Opaque (Reference) Tokens** for high-security M2M scenarios.
- **JWT**: Best for performance (stateless validation).
- **Opaque**: Best for security (instant revocation, no claims exposure to the client).

## 🚀 Getting Started
1. Update `appsettings.json` with your connection string.
2. Run `dotnet ef database update`.
3. Start the API: `dotnet run --project src/Auth.Api`.
