Extend the existing Authentication & Authorization module with an OPTIONAL document verification feature.

⚠️ Important:
- This feature must be low priority and optional
- It must NOT affect or complicate the existing authentication flow
- It must NOT introduce breaking changes
- It should integrate cleanly with the authorization layer only

--------------------------------------------------
📄 DOCUMENT VERIFICATION REQUIREMENTS
--------------------------------------------------

1. Admin Configuration

Allow company admins (tenant-based) to define required verification documents.

Each document must include:
- Name (e.g., National ID, Commercial Registration)
- Validation rules (e.g., ID/CR length — configurable dynamically)
- Assigned roles (many-to-many relationship)

Rules:
- A document can be required for multiple roles
- A role can require multiple documents

--------------------------------------------------

2. User Upload Flow

After registration and successful login:
- Users can upload required documents based on their assigned roles
- Each uploaded document should include metadata (not just file)

Documents must enter a:
- Pending state (awaiting review)

--------------------------------------------------

3. Admin Review Workflow

Admins can:
- View submitted documents
- Approve or reject each submission
- Add optional notes/comments

This process should behave like a request/approval system (similar to transaction approvals)

--------------------------------------------------

4. Verification Status

Each user should have a verification state:
- Pending
- Approved
- Rejected

--------------------------------------------------

5. Authorization Impact

- Users can authenticate normally (login is NOT affected)
- However, unverified users may be restricted from accessing certain endpoints

Implementation requirement:
- Enforce this via authorization policies (e.g., "VerifiedUser")
- NOT via authentication or JWT changes

--------------------------------------------------

6. Technical Constraints

- Do NOT store verification data in JWT
- Do NOT increase token size
- Keep verification logic isolated (modular design)
- Ensure minimal impact on existing architecture

--------------------------------------------------

7. Expected Output

Provide:
- Suggested entities (high-level only)
- How this integrates with authorization policies
- Minimal changes required to existing system
- Example of protecting endpoints using verification (policy-based)

Keep the solution lightweight, modular, and easy to enable/disable.