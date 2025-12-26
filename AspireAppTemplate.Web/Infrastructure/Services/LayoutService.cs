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

    public UserPreferences UserPreferences { get; private set; } = new();

    public bool ThemeDrawerOpen { get; set; }

    public event EventHandler? MajorUpdateOccurred;

    private void OnMajorUpdateOccurred() => MajorUpdateOccurred?.Invoke(this, EventArgs.Empty);

    public async Task InitializeAsync()
    {
        var prefs = await _localStorageService.GetItemAsync<UserPreferences>(StorageKey);
        if (prefs != null)
        {
            UserPreferences = prefs;
        }
    }

    public async Task ToggleDarkModeAsync()
    {
        UserPreferences.IsDarkMode = !UserPreferences.IsDarkMode;
        await SaveSettingsAsync();
    }

    public async Task ToggleDrawerAsync()
    {
        UserPreferences.IsDrawerOpen = !UserPreferences.IsDrawerOpen;
        await SaveSettingsAsync();
    }
    
    public async Task SetPrimaryColorAsync(string color)
    {
        UserPreferences.PrimaryColor = color;
        await SaveSettingsAsync();
    }

    public async Task SetBorderRadiusAsync(double radius)
    {
        UserPreferences.BorderRadius = radius;
        await SaveSettingsAsync();
    }

    private async Task SaveSettingsAsync()
    {
        await _localStorageService.SetItemAsync(StorageKey, UserPreferences);
        OnMajorUpdateOccurred();
    }

    public void ApplyUserPreferences(MudTheme theme)
    {
        theme.PaletteLight.Primary = UserPreferences.PrimaryColor;
        theme.PaletteDark.Primary = UserPreferences.PrimaryColor;
        theme.LayoutProperties.DefaultBorderRadius = $"{UserPreferences.BorderRadius}px";
    }
}
