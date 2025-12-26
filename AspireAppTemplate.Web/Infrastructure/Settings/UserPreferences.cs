namespace AspireAppTemplate.Web.Infrastructure.Settings;

public class UserPreferences
{
    public bool IsDarkMode { get; set; }
    public bool IsDrawerOpen { get; set; } = true;
    public string PrimaryColor { get; set; } = "#409EFF";
    public double BorderRadius { get; set; } = 4;
}
