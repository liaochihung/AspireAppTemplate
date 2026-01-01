using AspireAppTemplate.ApiService.Data;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.Profile.Get;

public record UserProfileResponse
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

public class Endpoint(AppDbContext dbContext) : EndpointWithoutRequest<UserProfileResponse>
{
    public override void Configure()
    {
        Get("/profile");
        Version(1);
        // Any authenticated user can view their own profile
    }

    public override async Task HandleAsync(CancellationToken ct)
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

        await SendAsync(new UserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        }, cancellation: ct);
    }
}
