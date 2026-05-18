
You are a senior .NET architect and identity/security expert.

Design a production-ready Authentication & Authorization module for ASP.NET Core (.NET 8+) using Clean Architecture, with a strong focus on authentication workflows, token efficiency, and extensibility.

--------------------------------------------------
🎯 PRIMARY FOCUS
--------------------------------------------------
Prioritize:
1. Authentication workflows (user + client)
2. JWT size optimization
3. Dynamic claims loading strategies
4. Developer usage guide (as if this is an SDK/module others will integrate)

--------------------------------------------------
🏗️ CONTEXT
--------------------------------------------------
Tech Stack:
- ASP.NET Core Web API (.NET 8+)
- Entity Framework Core
- ASP.NET Identity
- Clean Architecture
- Optional API Gateway (YARP/Ocelot compatible)

Architecture Requirements:  
- Modular design (plug-and-play module)  
- Repository Pattern  
- Service Layer using Result Pattern  
- DTOs for all external communication  
- Shared database with tenant isolation
--------------------------------------------------
🔐 AUTHENTICATION DESIGN
--------------------------------------------------

Design **complete authentication workflows** for:

1. 👤 USER AUTHENTICATION
- Email/password login
- JWT + Refresh token flow
- Forgot/reset password
- OAuth (Google, etc.)

2. 🤖 CLIENT AUTHENTICATION (Machine-to-Machine)
- Client Credentials Flow (manual implementation)
- client_id + client_secret
- Scoped access (scopes = client permissions)

For BOTH flows provide:
- Step-by-step sequence diagrams (textual)
- Code-level services
- Token issuing pipeline

 --------------------------------------------------

🧱 AUTHORIZATION SYSTEM
--------------------------------------------------  
Scope-Based Authorization:  
- Example scopes:  
- notifications.send  
- templates.manage


🔑 JWT STRATEGY (CRITICAL)
--------------------------------------------------

Goal: Keep JWT tokens **as small as possible**

Design TWO approaches:

1. MINIMAL TOKEN (Preferred)
- Include only:
  - sub
  - tenant_id
  - client_id (if applicable)
  - role or roles(if applicable)
- EXCLUDE roles & claims

2. ENRICHED TOKEN (Alternative)
- Include permissions(claims) + scopes(for client)

Explain trade-offs clearly.

--------------------------------------------------
⚡ DYNAMIC CLAIMS LOADING (CORE REQUIREMENT)
--------------------------------------------------

Design a system to load claims at runtime:

- Middleware placed between:
  Authentication → Authorization

Responsibilities:
- Fetch roles, permissions, tenant claims from DB/cache
- Attach claims to HttpContext.User dynamically

Consider:
- Performance (caching strategy: Redis/In-Memory)
- Selective claims loading (only what is needed per endpoint if possible)
- Extensibility

Answer explicitly:
- Is “on-demand claims loading per endpoint” feasible in ASP.NET Core?
- Recommended practical approach

Provide:
- Middleware implementation
- Claims transformation or alternative pattern
- Caching layer design

--------------------------------------------------
🏢 CLAIMS MODEL
--------------------------------------------------

Clearly define separation:

- User Claims → identity-level
- Role Claims → permission grouping
- Tenant Claims → tenant-scoped data
- Client Scopes → machine permissions (NOT mixed with user roles)

وضح العلاقات بينهم بالكود (Entity Design)

--------------------------------------------------
🏢 MULTI-TENANCY
--------------------------------------------------

- Tenant is nullable
- Users can belong to multiple tenants
- Roles are tenant-scoped

Provide:
- Tenant resolution (from JWT)
- Tenant context middleware
- EF Core filtering strategy

--------------------------------------------------
📦 INTEGRATION-FIRST DESIGN
--------------------------------------------------

Design this as a reusable module:

Focus on:
- Easy setup in new projects
- Minimal required configuration
- Sensible defaults
Explain:  
- Should it be:  
- NuGet package (SDK style), OR  
- Cloneable module (source-based)  
- Pros/cons of both approaches  
- Recommended structure for reuse  

Provide:
- IServiceCollection extensions
- Required configs (JWT, DB, Identity)
- Example integration in a fresh API

--------------------------------------------------
📘 DEVELOPER EXPERIENCE
--------------------------------------------------

Write a **clear Authentication Guide**:

- How login works internally
- How tokens are generated
- How claims are resolved
- How to add roles/claims/scopes
- How to register clients
- How to secure endpoints

--------------------------------------------------
⚠️ CONSTRAINTS
--------------------------------------------------
- Follow Clean Architecture strictly
- Avoid bloated JWTs
- Prefer runtime claim resolution when possible
- Design for scalability
- Keep module loosely coupled

--------------------------------------------------
🎁 OUTPUT FORMAT
--------------------------------------------------
1. Authentication Workflows (step-by-step)
2. JWT Strategy Comparison
3. Dynamic Claims Middleware Design (with code)
4. Entities & Relationships
5. Core Services
6. Integration Guide
7. Best Practices & Trade-offs