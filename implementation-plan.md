Extend the existing Authentication & Authorization module with a generalized Device Management and Request/Order Management subsystem.

⚠️ Important:
- Keep both features modular and optional
- Avoid tight coupling with authentication core
- Follow Clean Architecture principles
- Design for extensibility and dynamic business requirements

--------------------------------------------------
📱 DEVICE MANAGEMENT REQUIREMENTS
--------------------------------------------------

Implement a complete user device management system.

Goals:
- Control allowed number of devices per user
- Track active sessions/devices
- Allow user/admin device management
- Support configurable limits and expansion

--------------------------------------------------

1. DEVICE REGISTRATION & TRACKING

Each authenticated device/session should be registered.

Store:
- Device identifier
- Device name
- Platform / browser / OS info
- IP address (optional)
- Registration date
- Last activity
- Session status:
  - Active
  - Expired
  - Revoked
  - Blocked

--------------------------------------------------

2. DEVICE LIMIT MANAGEMENT

Support:
- Default allowed devices count
- Tenant-specific limits
- User-specific overrides
- Optional role-based limits

Behavior when limit exceeded:
- Reject login
OR
- Require removing an existing device/session

--------------------------------------------------

3. SESSION MANAGEMENT

Allow users/admins to:
- View all active sessions/devices
- Revoke sessions
- Force logout devices
- Rename trusted devices
- View login history

Explain:
- Relationship between sessions, refresh tokens, and devices
- Recommended token/session lifecycle strategy

--------------------------------------------------

4. DEVICE EXPANSION REQUESTS

A user may request:
- Increasing allowed device count
- Additional access/features

This should integrate with the generalized request/order system below.

--------------------------------------------------
🧾 GENERALIZED REQUEST / ORDER MANAGEMENT SYSTEM
--------------------------------------------------

Design a flexible system to handle dynamic business requests inside the platform.

Goal:
Provide a reusable workflow engine for requests such as:
- Increase device limit
- Request additional permissions
- Tenant feature activation
- Verification requests
- Custom business approvals
- Future dynamic workflows

--------------------------------------------------

1. REQUEST/ORDER CORE CONCEPT

A request/order should support:
- Type
- Status
- Requester
- Target entity
- Dynamic payload/data
- Approval workflow
- Notes/comments
- Audit history

Example statuses:
- Pending
- UnderReview
- Approved
- Rejected
- Cancelled

--------------------------------------------------

2. DYNAMIC REQUEST TYPES

The system must support adding new request types without major code changes.

Examples:
- DeviceLimitIncreaseRequest
- PermissionAccessRequest
- TenantFeatureRequest
- VerificationReviewRequest

Design approaches to consider:
- Polymorphism
- Dynamic payload storage (JSON)
- Strategy pattern / handlers
- Workflow abstraction

Explain the recommended approach.

--------------------------------------------------

3. APPROVAL WORKFLOW

Support:
- Admin review process
- Multi-step approvals (optional/extensible)
- Approval/rejection notes
- Audit tracking

Requests should behave similarly to:
- Transaction approval systems
- Ticket/request management workflows

--------------------------------------------------

4. SETTINGS MANAGEMENT

Introduce a dynamic settings/configuration system.

Requirements:
- Tenant-based settings
- System-wide settings
- Optional user-level overrides

Examples:
- Allowed device count
- Verification requirements
- Session timeout
- Feature toggles

Settings should support:
- Dynamic key/value structure
- Typed values if possible
- Caching strategy

--------------------------------------------------

5. AUTHORIZATION INTEGRATION

The request/order system should integrate with authorization policies.

Examples:
- Certain requests require specific roles
- Some requests require approval before permissions become active

--------------------------------------------------

6. ARCHITECTURE REQUIREMENTS

Design should include:
- Modular architecture
- Extensible workflows
- Minimal impact on existing auth system
- Event-driven approach if beneficial
- Clean separation between:
  - Authentication
  - Authorization
  - Session management
  - Requests/orders
  - Settings

--------------------------------------------------

7. EXPECTED OUTPUT

Provide:
- High-level architecture
- Suggested entities
- Relationships between:
  - Users
  - Devices
  - Sessions
  - Requests/orders
  - Settings
- Suggested workflow design
- Example request lifecycle
- Extensibility strategy
- Best practices for scalability and maintainability