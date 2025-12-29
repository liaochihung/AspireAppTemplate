using FastEndpoints;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Data.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Features.Identity.Sync;

public class Endpoint(AppDbContext dbContext) : EndpointWithoutRequest<UserProfileResponse>
{
    public override void Configure()
    {
        Post("/identity/sync");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            ThrowError("User ID claim is missing or invalid");
            return; // Explicit return to satisfy compiler flow analysis
        }

        var username = User.FindFirst("preferred_username")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        // Keycloak often puts first/last name in 'given_name' and 'family_name' claims, 
        // but depending on mapping they might be elsewhere. Standard OIDC claims:
        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;

        var user = await dbContext.Users.FindAsync([userId], cancellationToken: ct);

        if (user == null)
        {
            user = new AppUser
            {
                Id = userId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
        }
        else
        {
            user.LastLoginAt = DateTime.UtcNow;
            // Update info if changed in Keycloak
            if (!string.IsNullOrEmpty(username)) user.Username = username;
            if (!string.IsNullOrEmpty(email)) user.Email = email;
            if (!string.IsNullOrEmpty(firstName)) user.FirstName = firstName;
            if (!string.IsNullOrEmpty(lastName)) user.LastName = lastName;
        }

        await dbContext.SaveChangesAsync(ct);

        await SendAsync(new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            LastLoginAt = user.LastLoginAt
        }, cancellation: ct);
    }
}
