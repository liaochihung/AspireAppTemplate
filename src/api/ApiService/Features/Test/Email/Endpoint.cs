using AspireAppTemplate.ApiService.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace AspireAppTemplate.ApiService.Features.Test.Email;

[HttpPost("/api/test/email")]
[AllowAnonymous] // For easy testing, or secure it if preferred
public class Endpoint : EndpointWithoutRequest
{
    private readonly INotificationService _notificationService;

    public Endpoint(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var testEmail = "test@example.com";
        await _notificationService.SendWelcomeAsync(testEmail, "Test User");
        await SendOkAsync("Email queued for test@example.com");
    }
}
