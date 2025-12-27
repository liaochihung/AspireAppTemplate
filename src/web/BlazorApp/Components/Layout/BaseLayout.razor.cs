using AspireAppTemplate.Web.Infrastructure.Settings;
using AspireAppTemplate.Web.Infrastructure.Themes;
using Microsoft.AspNetCore.Components;
using LayoutServiceClass = AspireAppTemplate.Web.Infrastructure.Services.LayoutService;

namespace AspireAppTemplate.Web.Components.Layout
{
    public partial class BaseLayout : IDisposable
    {
        private readonly AppTheme _theme = new();
        private bool _isDarkMode;
        private bool _themeDrawerOpen;
        private UserPreferences _themePreference = new();

        protected override async Task OnInitializedAsync()
        {
            LayoutService.MajorUpdateOccurred += LayoutServiceOnMajorUpdateOccurred;

            _themePreference = await LayoutService.GetPreferenceAsync();
            _isDarkMode = _themePreference.IsDarkMode;
            LayoutServiceClass.ApplyUserPreferences(_theme, _themePreference);
        }

        private async void LayoutServiceOnMajorUpdateOccurred(object? sender, EventArgs e)
        {
            _themePreference = await LayoutService.GetPreferenceAsync();
            _isDarkMode = _themePreference.IsDarkMode;
            LayoutServiceClass.ApplyUserPreferences(_theme, _themePreference);
            await InvokeAsync(StateHasChanged);
        }

        private async Task ThemePreferenceChanged(UserPreferences preference)
        {
            _themePreference = preference;
            _isDarkMode = preference.IsDarkMode;
            LayoutServiceClass.ApplyUserPreferences(_theme, preference);
            await LayoutService.SetPreferenceAsync(preference);
            StateHasChanged();
        }

        public void OpenThemeDrawer()
        {
            _themeDrawerOpen = true;
            StateHasChanged();
        }

        public void Dispose()
        {
            LayoutService.MajorUpdateOccurred -= LayoutServiceOnMajorUpdateOccurred;
        }
    }
}
