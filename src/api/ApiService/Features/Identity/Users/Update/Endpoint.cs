using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Update;

public class UpdateUserRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore) : Endpoint<UpdateUserRequest>
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

        var result = await identityService.UpdateUserAsync(req.Id, user);
        
        if (!result.IsError)
        {
             // Sync local DB
             var userId = Guid.Parse(req.Id);
             var dbContext = Resolve<AspireAppTemplate.ApiService.Data.AppDbContext>();
             var appUser = await dbContext.Users.FindAsync([userId], cancellationToken: ct);
             
             if (appUser != null)
             {
                 appUser.Username = req.Username;
                 appUser.Email = req.Email;
                 appUser.FirstName = req.FirstName;
                 appUser.LastName = req.LastName;
                 await dbContext.SaveChangesAsync(ct);
             }
        }

        await cacheStore.EvictByTagAsync("users", ct);
        await this.SendResultAsync(result, ct: ct);
    }
}
