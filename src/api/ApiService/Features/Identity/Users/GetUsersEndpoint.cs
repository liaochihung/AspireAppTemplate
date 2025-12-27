using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using Keycloak.Net.Models.Users;

namespace AspireAppTemplate.ApiService.Features.Identity.Users;

public class GetUsersEndpoint : EndpointWithoutRequest<IEnumerable<User>>
{
    private readonly IdentityService _identityService;

    public GetUsersEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Get("/users");
        AllowAnonymous(); // For initial testing, or Replace with .RequireAuthorization()
        // Ideally: .Roles("admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Add search parameter support later
        var users = await _identityService.GetUsersAsync();
        await SendOkAsync(users, ct);
    }
}
