# Project Context

## Purpose
An Aspire-orchestrated .NET Solution template featuring a Blazor Web App frontend, a backend API, and Keycloak authentication. Designed to be a robust starting point for enterprise applications, following KISS principles without over-engineering (no Clean Architecture complexity).

## Tech Stack
- **Orchestration**: .NET Aspire
- **Backend**: .NET 9, ASP.NET Core Web API
- **Frontend**: Blazor Web App (Interactive Server), MudBlazor 8.2.0
- **Database**: PostgreSQL (via Aspire & Docker), Entity Framework Core
- **Identity**: Keycloak (Docker container), OpenID Connect (OIDC)
- **Testing**: xUnit (implied by `src/tests/`)

## Project Conventions

### Code Style
- Follow standard C# coding conventions.
- **Project Structure**:
  - `src/api/`: Backend services
  - `src/web/`: Frontend applications
  - `src/aspire/`: Aspire AppHost and ServiceDefaults
  - `src/shared/`: Shared libraries
- **Simplicity (KISS)**: Avoid unnecessary abstraction layers. Use direct EF Core injection in endpoints where appropriate.

### Architecture Patterns
- **Monorepo-style** resource management via Aspire.
- **Vertical Slice**-ish organization within the API is preferred over layered architecture.
- **Separation of Concerns**: Database logic in `AspireAppTemplate.Database`, but simpler integration allowed in newer iterations. *Note: GEMINI.md mentions merging DbContext back to ApiService for KISS.*

### Testing Strategy
- Integration tests via `AspireAppTemplate.Tests` (using `Aspire.Hosting.Testing` likely).

### Git Workflow
- Standard feature branches.
- Commit messages should be clear and descriptive.

## Domain Context
- **Identity Management**: Uses Keycloak. Users and Roles should ideally be managed via Keycloak Admin or a dedicated management UI in the app.
- **Multi-tenancy**: Evaluation phase (based on user history).

## Important Constraints
- **Ports**: PostgreSQL fixed to `5436` in dev for simplified tooling access.
- **Aesthetics**: Premium look and feel using MudBlazor with custom "Element Plus" styling.

## External Dependencies
- **PostgreSQL**: Data persistence.
- **Keycloak**: Identity Provider (IdP).
