using Blazored.LocalStorage;
using AspireAppTemplate.Web.Infrastructure.Settings;
using MudBlazor;

namespace AspireAppTemplate.Web.Infrastructure.Services;

public class LayoutService
{
    private readonly ILocalStorageService _localStorageService;
    private const string StorageKey = "userPreferences";

    public LayoutService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public bool ThemeDrawerOpen { get; set; }
    public bool IsDarkMode { get; set; }

    public event EventHandler? MajorUpdateOccurred;

    private void OnMajorUpdateOccurred() => MajorUpdateOccurred?.Invoke(this, EventArgs.Empty);

    public async Task<UserPreferences> GetPreferenceAsync()
    {
        try
        {
            var prefs = await _localStorageService.GetItemAsync<UserPreferences>(StorageKey);
            if (prefs != null)
            {
                IsDarkMode = prefs.IsDarkMode;
                return prefs;
            }
        }
        catch
        {
            // Ignore storage errors
        }
        
        // Return default with drawer open
        return new UserPreferences();
    }

    public async Task<bool> ToggleDrawerAsync()
    {
        var prefs = await GetPreferenceAsync();
        prefs.IsDrawerOpen = !prefs.IsDrawerOpen;
        await SetPreferenceAsync(prefs);
        return prefs.IsDrawerOpen;
    }

    public async Task<bool> ToggleDarkModeAsync()
    {
        var prefs = await GetPreferenceAsync();
        prefs.IsDarkMode = !prefs.IsDarkMode;
        IsDarkMode = prefs.IsDarkMode;
        await SetPreferenceAsync(prefs);
        return prefs.IsDarkMode;
    }

    public async Task SetDarkModeAsync(bool value)
    {
        var prefs = await GetPreferenceAsync();
        prefs.IsDarkMode = value;
        IsDarkMode = value;
        await SetPreferenceAsync(prefs);
    }

    public async Task SetPrimaryColorAsync(string color)
    {
        var prefs = await GetPreferenceAsync();
        prefs.PrimaryColor = color;
        await SetPreferenceAsync(prefs);
    }

    public async Task SetBorderRadiusAsync(double radius)
    {
        var prefs = await GetPreferenceAsync();
        prefs.BorderRadius = radius;
        await SetPreferenceAsync(prefs);
    }

    public async Task SetPreferenceAsync(UserPreferences prefs)
    {
        await _localStorageService.SetItemAsync(StorageKey, prefs);
        OnMajorUpdateOccurred();
    }

    public static void ApplyUserPreferences(MudTheme theme, UserPreferences prefs)
    {
        theme.PaletteLight.Primary = prefs.PrimaryColor;
        theme.PaletteLight.Secondary = prefs.SecondaryColor;
        theme.PaletteDark.Primary = prefs.PrimaryColor;
        theme.PaletteDark.Secondary = prefs.SecondaryColor;
        theme.LayoutProperties.DefaultBorderRadius = $"{prefs.BorderRadius}px";
    }
}

