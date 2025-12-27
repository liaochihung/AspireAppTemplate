using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Identity.Users;

public class GetUsersEndpoint : EndpointWithoutRequest<IEnumerable<KeycloakUser>>
{
    private readonly IdentityService _identityService;

    public GetUsersEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Get("/users");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _identityService.GetUsersAsync();
        await SendOkAsync(users, ct);
    }
}
