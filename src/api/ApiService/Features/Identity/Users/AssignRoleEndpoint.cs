using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Users;

public class UserRoleRequest
{
    public string Id { get; set; } = string.Empty; // From Route
    public string RoleName { get; set; } = string.Empty; // From Body
}

public class AssignRoleEndpoint : Endpoint<UserRoleRequest>
{
    private readonly IdentityService _identityService;

    public AssignRoleEndpoint(IdentityService identityService)
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
        await this.SendResultAsync(result);
    }
}
