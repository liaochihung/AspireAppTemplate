## 1. Backend Implementation
- [x] 1.1 Implement `IdentityService` in ApiService (Native HttpClient, no Keycloak.Net)
- [x] 1.2 Add Role Endpoints (REPR): `Create`, `Delete`, `GetAll` in `Features/Identity/Roles`
- [x] 1.3 Add User Endpoints: `Create`, `Delete`, `GetAll` in `Features/Identity/Users`
- [x] 1.4 Add User Role Endpoints: `AssignRoleEndpoint`
- [x] 1.5 Implement Policy Authority (`CanManageRoles`)

## 2. Frontend Implementation
- [ ] 2.1 Create `IdentityClient` in BlazorApp (Verify implementation)
- [x] 2.2 Create `Users.razor` page with list and search
- [x] 2.3 Create `UserDialog.razor` for creating users
- [x] 2.4 Create `Roles.razor` page with standard table layout
- [x] 2.5 Create `CreateRoleDialog.razor`
- [ ] 2.6 Implement Role Assignment UI (Button/Dialog in Users list or User Detail)

## 3. Verification
- [ ] 3.1 Verify User creation via UI syncs to Keycloak
- [ ] 3.2 Verify User deletion works
- [ ] 3.3 Verify Role creation via UI syncs to Keycloak
- [ ] 3.4 Verify Role assignment affects user permissions
