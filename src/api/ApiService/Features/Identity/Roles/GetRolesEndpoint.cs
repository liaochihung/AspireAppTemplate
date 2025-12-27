using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles;

public class GetRolesEndpoint : EndpointWithoutRequest<IEnumerable<KeycloakRole>>
{
    private readonly IdentityService _identityService;

    public GetRolesEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Get("/roles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var roles = await _identityService.GetRolesAsync();
        await SendOkAsync(roles, ct);
    }
}
