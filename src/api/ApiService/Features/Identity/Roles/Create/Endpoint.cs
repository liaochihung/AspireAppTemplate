using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

using AspireAppTemplate.ApiService.Infrastructure.Services;

namespace AspireAppTemplate.ApiService.Features.Identity.Roles.Create;

public class CreateRoleRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = default!;
    
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class CreateRoleValidator : Validator<CreateRoleRequest>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore, IAuditService auditService) : Endpoint<CreateRoleRequest>
{
    public override void Configure()
    {
        Post("/roles");
        Policies(AppPolicies.CanManageRoles);
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var role = new KeycloakRole 
        { 
            Name = req.Name,
            Description = req.Description 
        };
        var result = await identityService.CreateRoleAsync(role);
        
        if (!result.IsError)
        {
            await auditService.LogAsync("Create", "Role", req.Name, null, req, ct);
        }

        await cacheStore.EvictByTagAsync("roles", ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
