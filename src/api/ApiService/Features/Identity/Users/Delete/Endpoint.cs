using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Delete;

public class Endpoint : EndpointWithoutRequest
{
    private readonly IdentityService _identityService;

    public Endpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Delete("/users/{id}");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrEmpty(id))
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = await _identityService.DeleteUserAsync(id);
        await this.SendResultAsync(result);
    }
}
