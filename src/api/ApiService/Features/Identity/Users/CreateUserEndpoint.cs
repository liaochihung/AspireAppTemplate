using FastEndpoints;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using FluentValidation;

namespace AspireAppTemplate.ApiService.Features.Identity.Users;

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
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

public class CreateUserEndpoint : Endpoint<CreateUserRequest>
{
    private readonly IdentityService _identityService;

    public CreateUserEndpoint(IdentityService identityService)
    {
        _identityService = identityService;
    }

    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
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

        var success = await _identityService.CreateUserAsync(user);
        if (success)
        {
            await SendOkAsync(ct);
        }
        else
        {
            AddError("Failed to create user in Keycloak");
            await SendErrorsAsync(cancellation: ct);
        }
    }
}
