using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.Create;

public class CreateRoleRequest
{
    public string Name { get; set; } = default!;
}

public class CreateRoleValidator : Validator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class Endpoint(IdentityService identityService) : Endpoint<CreateRoleRequest>
{
    public override void Configure()
    {
        Post("/identity/roles");
        Policies(AppPolicies.CanManageRoles);
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var role = new KeycloakRole { Name = req.Name };
        var result = await identityService.CreateRoleAsync(role);
        await this.SendResultAsync(result, ct: ct);
    }
}
