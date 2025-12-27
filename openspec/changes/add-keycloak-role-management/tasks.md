## 1. Backend Implementation
- [ ] 1.1 Add `Keycloak.Net` package to ApiService
- [ ] 1.2 Create `IdentityService` in ApiService to interact with Keycloak Admin API (Users & Roles)
- [ ] 1.3 Add Role Endpoints: `GET /roles`, `POST /roles`, `DELETE /roles/{name}`
- [ ] 1.4 Add User Endpoints: `GET /users`, `POST /users`, `GET /users/{id}`, `PUT /users/{id}`, `DELETE /users/{id}`
- [ ] 1.5 Add User Role Endpoints: `POST /users/{id}/roles`, `DELETE /users/{id}/roles`

## 2. Frontend Implementation
- [ ] 2.1 Create `IdentityClient` (or separate Users/Roles clients) in BlazorApp
- [ ] 2.2 Create `Users.razor` page with list and search
- [ ] 2.3 Create `UserDialog.razor` for creating/editing users
- [ ] 2.4 Create `Roles.razor` page with standard table layout
- [ ] 2.5 Create `CreateRoleDialog.razor`
- [ ] 2.6 Update `UserDetail.razor` to include Role management tab

## 3. Verification
- [ ] 3.1 Verify User creation via UI syncs to Keycloak
- [ ] 3.2 Verify User deletion works
- [ ] 3.3 Verify Role creation via UI syncs to Keycloak
- [ ] 3.4 Verify Role assignment affects user permissions
