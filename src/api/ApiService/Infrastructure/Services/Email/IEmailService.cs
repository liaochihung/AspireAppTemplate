using MimeKit;

namespace AspireAppTemplate.ApiService.Infrastructure.Services.Email;

public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="htmlBody">Email body (HTML).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a pre-constructed MimeMessage.
    /// </summary>
    /// <param name="message">The MimeMessage to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(MimeMessage message, CancellationToken cancellationToken = default);
}
