## 1. Backend Implementation
- [x] 1.1 Implement `IdentityService` in ApiService (Native HttpClient, no Keycloak.Net)
- [x] 1.2 Add Role Endpoints (REPR): `Create`, `Delete`, `GetAll` in `Features/Identity/Roles`
- [x] 1.3 Add User Endpoints (REPR): `Create`, `Update`, `Delete`, `GetAll` in `Features/Identity/Users`
- [x] 1.4 Add User Role Endpoints: `AssignRole`, `RemoveRole`, `GetRoles`
- [x] 1.5 Implement Policy Authority (`CanManageRoles`, `CanManageUsers`)

## 2. Frontend Implementation
- [x] 2.1 Create `IdentityApiClient` in BlazorApp (Users & Roles methods)
- [x] 2.2 Create `Users.razor` page with list, search, and actions (Edit, Assign Roles)
- [x] 2.3 Create `UserDialog.razor` for creating/editing users (Password optional on edit)
- [x] 2.4 Create `Roles.razor` page with standard table layout
- [x] 2.5 Create `CreateRoleDialog.razor`
- [x] 2.6 Create `UserRolesDialog.razor` (List assigned roles, Add/Remove roles)

## 3. Verification
- [x] 3.1 Verify User creation via UI syncs to Keycloak
- [x] 3.2 Verify User deletion works
- [x] 3.3 Verify Role creation via UI syncs to Keycloak
- [x] 3.4 Verify Role assignment affects user permissions
