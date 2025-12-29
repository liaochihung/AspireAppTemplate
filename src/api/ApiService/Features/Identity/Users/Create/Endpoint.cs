using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Identity.Users.Create;

public class CreateUserRequest
{
    [System.Text.Json.Serialization.JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class Endpoint(IdentityService identityService, IOutputCacheStore cacheStore) : Endpoint<CreateUserRequest>
{
    public override void Configure()
    {
        Post("/users");
        Policies(AppPolicies.CanManageUsers);
    }

    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var user = new KeycloakUser
        {
            Username = req.Username,
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Enabled = true,
            EmailVerified = true,
            Credentials = new List<KeycloakCredential>
            {
                new KeycloakCredential { Type = "password", Value = req.Password, Temporary = false }
            }
        };

        var result = await identityService.CreateUserAsync(user);
        
        if (result.IsError)
        {
            await this.SendResultAsync(result, ct: ct);
            return;
        }

        // Sync to local DB
        var userId = Guid.Parse(result.Value);
        var appUser = new AspireAppTemplate.ApiService.Data.Entities.AppUser
        {
            Id = userId,
            Username = req.Username,
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var dbContext = Resolve<AspireAppTemplate.ApiService.Data.AppDbContext>();
        dbContext.Users.Add(appUser);
        await dbContext.SaveChangesAsync(ct);

        await cacheStore.EvictByTagAsync("users", ct);
        // Return Created with ID
        await SendAsync(new { Id = userId }, 201, cancellation: ct);
    }
}
