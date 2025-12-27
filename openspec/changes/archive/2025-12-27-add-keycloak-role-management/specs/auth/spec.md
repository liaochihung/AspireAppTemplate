## ADDED Requirements
### Requirement: Role Management
System Administrators SHALL be able to manage system roles directly from the application.

#### Scenario: List all roles
- **WHEN** Admin navigates to Role Management page
- **THEN** A list of all available roles from Keycloak IS displayed

#### Scenario: Create new role
- **WHEN** Admin submits a valid new role name
- **THEN** The role IS created in Keycloak
- **AND** The role list IS updated

#### Scenario: Delete role
- **WHEN** Admin confirms deletion of a role
- **THEN** The role IS removed from Keycloak

### Requirement: User Role Assignment
System Administrators SHALL be able to assign and revoke roles for users.

#### Scenario: Assign role to user
- **WHEN** Admin selects a role to add to a user
- **THEN** The user IS granted that role in Keycloak

#### Scenario: Revoke role from user
- **WHEN** Admin removes a role from a user
- **THEN** The user NO LONGER possesses that role
