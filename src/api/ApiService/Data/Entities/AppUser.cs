using System.ComponentModel.DataAnnotations;

namespace AspireAppTemplate.ApiService.Data.Entities;

public class AppUser
{
    [Key]
    public Guid Id { get; set; } // Matches Keycloak User ID (sub claim)

    [Required]
    [MaxLength(150)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}
