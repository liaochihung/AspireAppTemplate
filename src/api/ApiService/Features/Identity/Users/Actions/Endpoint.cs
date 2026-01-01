using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FastEndpoints;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Actions;

public record UserActionRequest
{
    public required List<string> Actions { get; init; }
}

public class Endpoint(IdentityService identityService) : Endpoint<UserActionRequest>
{
    public override void Configure()
    {
        Put("/users/{id}/actions");
        // Allowing users to trigger actions for themselves or Admin managing users
        // For simplicity here, we stick to generic policy, but typically users should be able to trigger their own PwReset
        // However, this API is "admin trigger email", so usually needs admin rights OR we check user ownership.
        // Let's rely on checking ID ownership in HandleAsync or generic Auth policy.
        Policies(AppPolicies.CanManageUsers); 
        Version(1);
    }

    public override async Task HandleAsync(UserActionRequest req, CancellationToken ct)
    {
        var id = Route<string>("id");
        // Security Check: Allow if Admin OR if the target ID matches current user
        var currentUserId = User.FindFirst("sub")?.Value;
        var isAdmin = User.IsInRole("Administrator"); // Simplified role check

        // In a real app, use a Requirement/Policy handler. 
        // For now, if not admin and not self, forbid.
        if (!isAdmin && currentUserId != id)
        {
            await SendForbiddenAsync(ct);
            return;
        }

        var result = await identityService.ExecuteActionsEmailAsync(id, req.Actions);
        await this.SendResultAsync(result);
    }
}
