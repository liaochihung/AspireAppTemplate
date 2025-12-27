using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Users;

public class DeleteUserEndpoint : EndpointWithoutRequest
{
    private readonly IdentityService _identityService;

    public DeleteUserEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Delete("/users/{id}");
        AllowAnonymous();
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
