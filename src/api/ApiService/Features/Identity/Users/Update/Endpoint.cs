using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Update;

public class UpdateUserRequest
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}

public class Endpoint(IdentityService identityService) : Endpoint<UpdateUserRequest>
{
    public override void Configure()
    {
        Put("/users/{id}");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var user = new KeycloakUser
        {
            Id = req.Id,
            Username = req.Username,
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Enabled = req.Enabled
        };

        var success = await identityService.UpdateUserAsync(req.Id, user);
        
        if (success)
            await SendOkAsync(ct);
        else
        {
            AddError("Failed to update user");
            await SendErrorsAsync(cancellation: ct);
        }
    }
}
