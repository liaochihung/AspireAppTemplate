using System.ComponentModel.DataAnnotations;

namespace AspireAppTemplate.ApiService.Data.Entities;

public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty; // Keycloak User ID

    [Required]
    [MaxLength(150)]
    public string UserName { get; set; } = string.Empty; // Snapshot of User Name

    [Required]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty; // e.g. Create, Update, Delete

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty; // e.g. Product

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty; // Primary Key

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
}
