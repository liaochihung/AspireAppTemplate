using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.GetAll;

public class Endpoint : EndpointWithoutRequest<IEnumerable<KeycloakUser>>
{
    private readonly IdentityService _identityService;

    public Endpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Get("/users");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _identityService.GetUsersAsync();
        await this.SendResultAsync(result);
    }
}
