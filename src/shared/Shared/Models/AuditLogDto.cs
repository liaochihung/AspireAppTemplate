namespace AspireAppTemplate.Shared.Models;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
}
