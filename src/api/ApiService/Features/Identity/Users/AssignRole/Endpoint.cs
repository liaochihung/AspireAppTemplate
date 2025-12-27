using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.AssignRole;

public class UserRoleRequest
{
    public string Id { get; set; } = string.Empty; // From Route
    public string RoleName { get; set; } = string.Empty; // From Body
}

public class Endpoint : Endpoint<UserRoleRequest>
{
    private readonly IdentityService _identityService;

    public Endpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/users/{id}/roles");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(UserRoleRequest req, CancellationToken ct)
    {
        var result = await _identityService.AssignRoleToUserAsync(req.Id, req.RoleName);
        await this.SendResultAsync(result, ct: ct);
    }
}
