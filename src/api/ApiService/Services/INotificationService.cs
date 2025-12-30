// using AspireAppTemplate.Shared.Auth;

namespace AspireAppTemplate.ApiService.Services;

public interface INotificationService
{
    /// <summary>
    /// Sends a generic email notification.
    /// This method should enqueue a background job.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string message);

    /// <summary>
    /// Sends a welcome email to a new user.
    /// </summary>
    Task SendWelcomeAsync(string email, string displayName);
}
