namespace AspireAppTemplate.Shared;

public class KeycloakUser
{
    public string? Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public List<KeycloakCredential>? Credentials { get; set; }
}

public class KeycloakCredential
{
    public string Type { get; set; } = "password";
    public string Value { get; set; } = string.Empty;
    public bool Temporary { get; set; } = false;
}

public class KeycloakRole
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class TokenResponse
{
    public string access_token { get; set; } = string.Empty;
    public int expires_in { get; set; }
}
