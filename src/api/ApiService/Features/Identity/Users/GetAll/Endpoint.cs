using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.GetAll;

public class Endpoint(IdentityService identityService) : Endpoint<PaginationRequest, PaginatedResult<KeycloakUser>>
{
    public override void Configure()
    {
        Get("/users");
        Policies(AppPolicies.CanManageUsers);
        Options(x => x.CacheOutput(c => c.Expire(TimeSpan.FromMinutes(5)).Tag("users")));
    }

    public override async Task HandleAsync(PaginationRequest req, CancellationToken ct)
    {
        var result = await identityService.GetUsersAsync(req.SearchTerm);
        
        if (result.IsError)
        {
            await SendAsync(new PaginatedResult<KeycloakUser>(), cancellation: ct);
            return;
        }

        var keycloakUsers = result.Value.ToList();
        var userIds = new List<Guid>();
        foreach (var u in keycloakUsers)
        {
            if (!string.IsNullOrEmpty(u.Id) && Guid.TryParse(u.Id, out var parsedId))
            {
                userIds.Add(parsedId);
            }
        }

        if (userIds.Any())
        {
            var dbContext = Resolve<AspireAppTemplate.ApiService.Data.AppDbContext>();
            var localUsers = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                dbContext.Users.Where(u => userIds.Contains(u.Id)), 
                cancellationToken: ct);

            foreach (var kUser in keycloakUsers)
            {
                if (Guid.TryParse(kUser.Id, out var uid))
                {
                    var local = localUsers.FirstOrDefault(l => l.Id == uid);
                    if (local != null)
                    {
                        kUser.LastLoginAt = local.LastLoginAt;
                        kUser.CreatedAt = local.CreatedAt;
                    }
                }
            }
        }

        var paginated = keycloakUsers.ToPaginatedResult(req);
        await SendAsync(paginated, cancellation: ct);
    }
}
