using AspireAppTemplate.Web.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        private bool _drawerOpen;
        private bool _isDarkMode;

        [CascadingParameter]
        public BaseLayout? ParentLayout { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var prefs = await LayoutService.GetPreferenceAsync();
            _drawerOpen = prefs.IsDrawerOpen;
            _isDarkMode = prefs.IsDarkMode;

            LayoutService.MajorUpdateOccurred += OnMajorUpdateOccurred;
        }

        private async Task DrawerToggle()
        {
            _drawerOpen = await LayoutService.ToggleDrawerAsync();
        }

        private async Task ToggleDarkMode()
        {
            _isDarkMode = !_isDarkMode;
            await LayoutService.SetDarkModeAsync(_isDarkMode);
        }

        private void OpenThemeDrawer()
        {
            ParentLayout?.OpenThemeDrawer();
        }

        private void OnMajorUpdateOccurred(object? sender, EventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            LayoutService.MajorUpdateOccurred -= OnMajorUpdateOccurred;
        }
    }
}
