namespace AspireAppTemplate.ApiService.Infrastructure.Services.Email;

public class EmailSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025; // Default to MailHog/Papercut port
    public string SenderName { get; set; } = "Aspire App";
    public string SenderEmail { get; set; } = "no-reply@example.com";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = false;
}
