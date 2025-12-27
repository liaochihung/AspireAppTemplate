using FastEndpoints;
using AspireAppTemplate.ApiService.Services;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles;

public class DeleteRoleEndpoint : EndpointWithoutRequest
{
    private readonly IdentityService _identityService;

    public DeleteRoleEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Delete("/roles/{name}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var name = Route<string>("name");
        var success = await _identityService.DeleteRoleAsync(name!);
        
        if (success)
            await SendOkAsync(ct);
        else
            await SendNotFoundAsync(ct);
    }
}
