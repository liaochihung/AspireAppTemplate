using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FastEndpoints;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Profile.Update;

public record UpdateProfileRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

public class Endpoint(
    AppDbContext dbContext, 
    IdentityService identityService) : Endpoint<UpdateProfileRequest>
{
    public override void Configure()
    {
        Put("/profile");
        Version(1);
        // Any authenticated user can update their own profile
    }

    public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var user = await dbContext.Users.FindAsync([userId], cancellationToken: ct);
        if (user == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // Update Keycloak first (keeping Username and Email unchanged)
        var keycloakUser = new KeycloakUser
        {
            Id = userIdClaim,
            Username = user.Username, // Keep existing
            Email = user.Email,       // Keep existing
            FirstName = req.FirstName,
            LastName = req.LastName,
            Enabled = true            // Keep enabled status
        };

        var result = await identityService.UpdateUserAsync(userIdClaim, keycloakUser);

        if (result.IsError)
        {
            await this.SendResultAsync(result);
            return;
        }

        // If Keycloak update succeeded, update local DB
        user.FirstName = req.FirstName;
        user.LastName = req.LastName;
        await dbContext.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}
