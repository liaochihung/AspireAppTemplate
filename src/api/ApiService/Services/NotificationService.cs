using AspireAppTemplate.ApiService.Infrastructure.Services.Email;
using Hangfire;

namespace AspireAppTemplate.ApiService.Services;

public class NotificationService : INotificationService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IBackgroundJobClient backgroundJobClient, ILogger<NotificationService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string message)
    {
        _logger.LogInformation("Enqueuing email to {To}", to);
        
        // Use Hangfire to enqueue the sending task
        // NOTE: The lambda expression here is serialized by Hangfire. 
        // It's important that IEmailService is registered in the DI container so Hangfire server can resolve it.
        _backgroundJobClient.Enqueue<IEmailService>(service => 
            service.SendEmailAsync(to, subject, message, CancellationToken.None));

        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(string email, string displayName)
    {
        var subject = "Welcome to Aspire App Template!";
        var body = $@"
            <h1>Welcome, {displayName}!</h1>
            <p>We are excited to have you on board.</p>
            <p>Best Regards,<br/>Aspire App Template Project</p>";

        return SendEmailAsync(email, subject, body);
    }
}
