using System.Text.Json.Serialization;

namespace AspireAppTemplate.ApiService.Features.Identity.Sync;

public class UserProfileResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
    
    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
}
