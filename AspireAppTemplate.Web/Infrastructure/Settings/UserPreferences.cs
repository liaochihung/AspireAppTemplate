using AspireAppTemplate.Web.Infrastructure.Themes;

namespace AspireAppTemplate.Web.Infrastructure.Settings;

public class UserPreferences
{
    public bool IsDarkMode { get; set; }
    public bool IsDrawerOpen { get; set; } = true;
    public string PrimaryColor { get; set; } = CustomColors.Light.Primary;
    public string SecondaryColor { get; set; } = CustomColors.Light.Secondary;
    public double BorderRadius { get; set; } = 4;
    public TablePreference TablePreference { get; set; } = new();
}
