using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using Keycloak.Net.Models.Roles;
using FluentValidation;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles;

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateRoleValidator : Validator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class CreateRoleEndpoint : Endpoint<CreateRoleRequest>
{
    private readonly IdentityService _identityService;

    public CreateRoleEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/roles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var role = new Role
        {
            Name = req.Name,
            Description = req.Description
        };

        var success = await _identityService.CreateRoleAsync(role);
        if (success)
        {
            await SendOkAsync(ct);
        }
        else
        {
            AddError("Failed to create role in Keycloak");
            await SendErrorsAsync(cancellation: ct);
        }
    }
}
