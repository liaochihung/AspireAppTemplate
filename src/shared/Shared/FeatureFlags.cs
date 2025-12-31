namespace AspireAppTemplate.Shared;

/// <summary>
/// Feature flags to control application behavior based on environment capabilities
/// </summary>
public class FeatureFlags
{
    /// <summary>
    /// Enables email-based features (password reset, 2FA setup via email)
    /// Set to false in offline environments where SMTP is unavailable
    /// </summary>
    public bool EnableEmailFeatures { get; set; } = true;

    /// <summary>
    /// Enables social login providers (Google, Facebook, etc.)
    /// Set to false in environments without internet access
    /// </summary>
    public bool EnableSocialLogin { get; set; } = true;
}
