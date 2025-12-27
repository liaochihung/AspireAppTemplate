# Change: Add Keycloak Identity Management (Users & Roles)

## Why
Currently, managing users and roles requires access to the Keycloak Admin Console, which is inconvenient for application administrators and exposes too much complexity. We need a native UI within the application to create users, manage roles, and assign them.

## What Changes
- Add backend API endpoints to proxy Keycloak User API (Get, Create, Update, Delete, Reset Password).
- Add backend API endpoints to proxy Keycloak Role API (Get, Create, Delete, Assign).
- Add frontend "Users" management page in the Admin area.
- Add frontend "Roles" management page in the Admin area.
- Add "Role Assignment" UI to User details page.

## Impact
- **Affected Specs**: `auth`
- **Affected Code**: `src/api/ApiService`, `src/web/BlazorApp`
