# NeoAuth.Core 🛡️

**NeoAuth.Core** is a high-performance, enterprise-grade Authentication and Authorization module for .NET 8. Built using Clean Architecture, it supports Hierarchical Claims, standard OAuth2/OIDC patterns, advanced Device Management, dynamic Document Validation, and containerized deployment with an Ocelot API Gateway.

## 🚀 Key Features

- **Hierarchical Security**: Recursive expansion of Permissions and Scopes (e.g., `Product.Manage` -> `product.read`, `product.add`).
- **Device Management**: Track user devices, enforce strict dynamic device limits (Global > Tenant > Role > User), and verify new devices via OTP over email before granting access.
- **Advanced Document Metadata**: Dynamically validate user-uploaded documents with customizable JSON rules (RegEx, length constraints, issue-to-expiration bounds).
- **Dual Token Strategy**: Seamlessly issues and refreshes both **JWTs** (for standard stateless auth) and **Reference/Opaque Tokens** (for high-security M2M scenarios with instant revocation).
- **M2M Support**: Full Client Credentials flow with client-specific scope management.
- **Docker Orchestration**: Fully containerized environment orchestrating SQL Server, the Auth API, and an Ocelot API Gateway using Docker Compose.
- **Result Pattern**: Consistent and predictable API responses using the `Result<T>` structural shape.

## 🛠️ Architecture Overview

The solution follows Clean Architecture principles:
- **`Auth.Domain`**: Core entities, interfaces, and business rules (e.g., `ApplicationUser`, `UserDevice`, `RefreshToken`).
- **`Auth.Application`**: Application services, DTOs, and interface definitions (e.g., `AuthService`, `DeviceService`).
- **`Auth.Infrastructure`**: EF Core configurations, SQL Server database context, caching, and implementations of Domain interfaces.
- **`Auth.Api`**: Main RESTful API, exposing endpoints and handling authentication middleware.
- **`Auth.Gateway`**: Ocelot API Gateway that routes traffic to the underlying microservices.

## 🐳 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://www.docker.com/) (For containerized deployment)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (If running locally without Docker)

## 🚦 Getting Started

There are two primary ways to run NeoAuth.Core: natively or via Docker.

### Option 1: Docker Compose (Recommended)

This is the easiest way to spin up the entire ecosystem, including the database and API Gateway.

```bash
# 1. Clone the repository
git clone https://github.com/OND10/NeoAuth.Core.git
cd NeoAuth.Core

# 2. Spin up the containers (SQL Server, Auth API, and Gateway)
docker compose up --build -d

# The API is now accessible via the Gateway at http://localhost:5000
```
*Note: EF Core Migrations are applied automatically upon application startup.*

### Option 2: Running Locally (.NET CLI)

1. **Update Connection String**: Modify `src/Auth.Api/appsettings.json` to point to your local SQL Server instance.
2. **Apply Database Migrations**:
   ```bash
   dotnet ef database update --project src/Auth.Infrastructure --startup-project src/Auth.Api
   ```
3. **Run the API**:
   ```bash
   dotnet run --project src/Auth.Api
   ```
4. **(Optional) Run the Gateway**:
   ```bash
   dotnet run --project src/Auth.Gateway
   ```

## 📖 API Documentation & Postman

A full Postman collection is included in the root directory: `postman_collection.json`. You can import this directly into Postman to explore all endpoints.

### Key Endpoint Groups:
- **Auth (`/api/auth`)**: Login, Register, Refresh Token, Forgot Password, Google Auth.
- **Clients (`/api/client`)**: Create M2M clients, authenticate via Client Credentials, rotate secrets.
- **Devices (`/api/device`)**: Fetch user devices, revoke devices. *(Note: New devices during login require `/api/auth/verify-device` with an OTP).*
- **Documents (`/api/document`)**: Setup required documents, submit bulk verifications, process documents.

## 📄 Advanced Document Validation Rules

When setting up a new required document (e.g., National ID, Passport) via `POST /api/document/configure`, you have granular control over the data submitted by users. If a document is flagged with `requiresDocumentNumber`, `requiresIssueDate`, or `requiresExpiryDate`, you can specify exactly what those values should look like using the `validationRulesJson` field.

The `validationRulesJson` field expects a serialized JSON string representing the `DocumentValidationRules` object:

```csharp
public class DocumentValidationRules
{
    public int? DocumentNumberMinLength { get; set; }
    public int? DocumentNumberMaxLength { get; set; }
    public string? DocumentNumberRegex { get; set; }
    public int? MinDurationDays { get; set; }
    public int? MaxDurationDays { get; set; }
    public int? MinDurationYears { get; set; }
    public int? MaxDurationYears { get; set; }
}
```

### Examples of Business Rules

**1. Managing Document Length and Format (Regex):**
If you want to ensure a National ID document is exactly 14 digits long, and only contains numbers, your `validationRulesJson` would look like this:
```json
"validationRulesJson": "{\"DocumentNumberMinLength\": 14, \"DocumentNumberMaxLength\": 14, \"DocumentNumberRegex\": \"^\\\\d{14}$\"}"
```

**2. Managing the Period Between Issue and Expiry Dates:**
If a Passport *must* have an exact validity period of 10 years (e.g., issued on `05-11-2025` and expiring on `05-11-2035`), you can strictly enforce this using `MinDurationYears` and `MaxDurationYears`:
```json
"validationRulesJson": "{\"MinDurationYears\": 10, \"MaxDurationYears\": 10}"
```
Alternatively, for short-term documents (like a 30-day visa), you can use the days constraint:
```json
"validationRulesJson": "{\"MinDurationDays\": 30, \"MaxDurationDays\": 30}"
```

**Full JSON Request Example:**
```json
{
  "name": "National ID Card",
  "description": "Please upload a clear scan of your National ID.",
  "minFilesRequired": 2,
  "targetRoleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "triggerRoleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isActive": true,
  "requiresDocumentNumber": true,
  "requiresIssueDate": true,
  "requiresExpiryDate": true,
  "validationRulesJson": "{\"DocumentNumberMinLength\": 14, \"DocumentNumberMaxLength\": 14, \"DocumentNumberRegex\": \"^\\\\d{14}$\", \"MinDurationYears\": 10, \"MaxDurationYears\": 10}"
}
```

## 🤝 Contributing

We welcome collaboration to make NeoAuth.Core even better! 

### Contribution Workflow:
1. **Fork the Repository** and clone your fork locally.
2. **Create a Feature Branch** (`git checkout -b feature/amazing-feature`).
3. **Follow Clean Architecture**: Ensure your code respects the boundaries (no DB logic in the Domain layer, etc.).
4. **Write Tests**: New features should be accompanied by xUnit tests in the `Auth.Tests` project.
5. **Commit your Changes** using conventional commits (`feat: add new feature`, `fix: resolve bug`).
6. **Push to the Branch** (`git push origin feature/amazing-feature`).
7. **Open a Pull Request** against the `main` branch.

### Running Tests
Before submitting a pull request, ensure all tests pass cleanly:
```bash
dotnet test
```

## 📝 License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
