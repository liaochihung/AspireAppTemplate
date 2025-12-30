using AspireAppTemplate.ApiService.Services;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.Test.Email;

public class Endpoint(INotificationService notificationService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/test/email");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var testEmail = "test@example.com";
        await notificationService.SendWelcomeAsync(testEmail, "Test User");
        await SendOkAsync($"Email queued for {testEmail}", ct);
    }
}
