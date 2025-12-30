using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AspireAppTemplate.ApiService.Infrastructure.Services.Email;

public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        await SendEmailAsync(message, cancellationToken);
    }

    public async Task SendEmailAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient();

        try
        {
            _logger.LogInformation("Connecting to SMTP server {Host}:{Port} (SSL: {EnableSsl})...", _settings.Host, _settings.Port, _settings.EnableSsl);
            
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                _logger.LogInformation("Authenticating with SMTP server...");
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            _logger.LogInformation("Sending email to {To}...", message.To);
            await client.SendAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully.");

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Error: {Message}", message.To, ex.Message);
            throw;
        }
    }
}
