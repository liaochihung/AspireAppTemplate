# Change: Add Keycloak Identity Management (Users & Roles)

## Why
Currently, managing users and roles requires access to the Keycloak Admin Console, which is inconvenient for application administrators and exposes too much complexity. We need a native UI within the application to create users, manage roles, and assign them.

## What Changes
- **Backend**:
    - Implement `IdentityService` using native `HttpClient` to talk to Keycloak Admin REST API (removing need for `Keycloak.Net`).
    - Add Role management endpoints using **REPR Pattern** (`Features/Identity/Roles/...`).
    - Add User management endpoints (`Features/Identity/Users/...`).
    - Secure endpoints using Policy-based authorization (`CanManageRoles`, `CanManageUsers`).
- **Frontend**:
    - Add "Users" management page (`Users.razor`) and Dialog (`UserDialog.razor`).
    - Add "Roles" management page (`Roles.razor`) and Dialog (`CreateRoleDialog.razor`).
    - Add "Role Assignment" capability (Pending).

## Impact
- **Affected Specs**: `auth`
- **Affected Code**: 
    - `src/api/ApiService/Features/Identity`: New feature folders.
    - `src/shared/Shared`: Keycloak models.
    - `src/web/BlazorApp/Components/Pages/Admin`: New admin pages.
