using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;

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

public class Endpoint : Endpoint<CreateUserRequest>
{
    private readonly IdentityService _identityService;

    public Endpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

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

        var result = await _identityService.CreateUserAsync(user);
        await this.SendResultAsync(result, ct: ct);
    }
}
