# NeoAuth Module - Developer Guide

## Overview
NeoAuth is a modular, secure, and extensible Authentication and Authorization module built with ASP.NET Core 8 following Clean Architecture principles.

## Architecture

The module is divided into four main layers:

1. **Domain Layer (`Auth.Domain`)**: Contains the core entities (`ApplicationUser`, `Tenant`, `UserDevice`, `UserDocument`), Enums, and repository interfaces. All entities inherit from `AuditableEntity` (Id, CreatedAt, UpdatedAt, IsDeleted).
2. **Application Layer (`Auth.Application`)**: Contains Use Cases, Services (`AuthService`, `DeviceService`, `DocumentService`), DTOs, and Interfaces (`ITokenService`, `IEmailService`). 
3. **Infrastructure Layer (`Auth.Infrastructure`)**: Implements the repository interfaces using EF Core and ASP.NET Core Identity. It also includes the `AuthDbContext` and EF Core entity configurations.
4. **API Layer (`Auth.Api`)**: Contains Controllers, Middleware (e.g., `ClaimsEnrichmentMiddleware`), and the Dependency Injection extension methods (`AuthModuleExtensions`).

## Core Features

### 1. Authentication & Tokens
- **JWT & Reference Tokens**: Supports standard JWTs and opaque reference tokens for API client-to-client communication.
- **Refresh Tokens**: Token rotation is implemented to maintain long-lived sessions securely.
- **Google OAuth**: Integrated endpoint for Google Sign-In.

### 2. Device Management
The Device Management feature allows tracking and limiting the number of active devices per user.

- **Limit Policies**: Limits can be configured globally (default is 5), per-tenant, or per-role.
- **Verification Flow**: 
  1. User logs in with a new `DeviceFingerprint`.
  2. `AuthService` registers the device as `PendingVerification`.
  3. A 6-digit OTP is generated using ASP.NET Identity's `TokenOptions.DefaultEmailProvider`.
  4. The user receives the OTP via email and a short-lived `TemporaryToken` is returned in the API response.
  5. The user submits the OTP to `/api/auth/verify-device` along with the temporary token to activate the device and receive full tokens.
- **Eviction**: If a user hits their device limit, the oldest active device is automatically revoked.

### 3. Document Management
Handles dynamic document requirements (e.g., KYC).
- **Dynamic Definitions**: `RequiredDocument` defines rules like `MinFilesRequired` and expiration policies.
- **Bulk Upload**: A specialized `DocumentService` handles multipart form data to process multiple files, save them locally, and map them to the corresponding `RequiredDocument` definitions in a single transaction.

## Setup & Testing

### Running Migrations
1. Open the terminal.
2. Ensure you are in the `src/Auth.Infrastructure` folder or specify the startup project.
3. Run: `dotnet ef migrations add InitialCreate -s ../Auth.Gateway`

### Running Tests
The `tests/Auth.Tests` project contains unit tests using `xUnit`, `Moq`, and `FluentAssertions`.
- `dotnet test` will run all suites.
- When contributing to services like `DeviceService`, ensure you mock the repository layer and assert business logic (like device limits and pending status).

## Contributing Guidelines

1. **Clean Architecture**: Never leak Domain Entities into the API layer. Always use Record DTOs in the Application layer.
2. **Nullable Reference Types**: Ensure strict null checking. Use `?` for optional properties and handle nulls explicitly.
3. **Result Pattern**: Use the `Result` and `Result<T>` wrapper for service layer returns. Never throw exceptions for business logic errors (e.g., "User not found"); return `Result.Failure(Error)`.
4. **Dependency Injection**: Add new services to `AuthModuleExtensions.cs`.
