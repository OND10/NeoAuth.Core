Extend the existing Authentication & Authorization module with an OPTIONAL device management and device limit feature.

⚠️ Important:
- This feature must remain optional and loosely coupled
- It must NOT complicate the core authentication flow
- It must NOT introduce breaking changes
- It should integrate mainly with session/authentication management
- Keep implementation lightweight and extensible

--------------------------------------------------
📱 DEVICE MANAGEMENT REQUIREMENTS
--------------------------------------------------

Implement a mechanism to control how many devices a user can use simultaneously.

Goal:
- A user can only access the system from a limited number of registered devices
- Device limits should be configurable

--------------------------------------------------

1. DEVICE REGISTRATION

After successful login:
- The current device should be identified and registered for the user

Examples:
- Mobile device
- Browser/device combination
- Desktop application

Each registered device should contain:
- Device identifier
- Device name / friendly name
- Platform / OS / browser info
- Registration date
- Last activity date
- Status (Active, Revoked, Blocked)

--------------------------------------------------

2. DEVICE IDENTIFICATION

Design a practical approach for identifying devices.

Consider:
- Browser fingerprint/device fingerprint
- Mobile device identifiers
- Generated persistent device ID

⚠️ Important:
- Do NOT rely on MAC addresses directly for browsers because browsers cannot reliably expose MAC addresses securely
- Explain recommended alternatives for web and mobile applications

Provide:
- Recommended implementation strategy
- Security considerations
- Reliability trade-offs

--------------------------------------------------

3. DEVICE LIMIT POLICY

Support:
- Maximum allowed devices per user
- Tenant-level configurable limits
- Optional role-based overrides

Behavior:
- If limit is exceeded:
  - Reject login, OR
  - Require replacing/removing an existing device

--------------------------------------------------

4. DEVICE MANAGEMENT FEATURES

Allow users/admins to:
- View registered devices
- Revoke devices
- Rename devices
- Force logout from specific devices

--------------------------------------------------

5. AUTHENTICATION INTEGRATION

Explain how device validation integrates with:
- JWT authentication
- Refresh tokens
- Session management

Requirements:
- Device validation should happen during authentication/token refresh
- Tokens should be associated with a registered device
- Revoked devices should no longer refresh tokens

--------------------------------------------------

6. SECURITY CONSIDERATIONS

Address:
- Device spoofing risks
- Browser fingerprint limitations
- Refresh token theft
- Trusted device strategy

--------------------------------------------------

7. ARCHITECTURE CONSTRAINTS

- Keep device management modular and optional
- Avoid tight coupling with core identity logic
- Do NOT bloat JWT tokens unnecessarily
- Follow Clean Architecture principles

--------------------------------------------------

8. EXPECTED OUTPUT

Provide:
- High-level entity design
- Suggested authentication workflow updates
- Device validation flow
- Refresh token/device relationship
- Example authorization/session checks
- Recommended implementation strategy for:
  - Web
  - Mobile
  - Desktop