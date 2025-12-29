using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace AspireAppTemplate.Shared;

public class KeycloakUser
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }
    
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
    
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
    
    [JsonPropertyName("emailVerified")]
    public bool EmailVerified { get; set; } = false;
    
    [JsonPropertyName("credentials")]
    public List<KeycloakCredential>? Credentials { get; set; }

    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }
}

public class KeycloakCredential
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "password";
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
    
    [JsonPropertyName("temporary")]
    public bool Temporary { get; set; } = false;
}

public class KeycloakRole
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string access_token { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int expires_in { get; set; }
}
