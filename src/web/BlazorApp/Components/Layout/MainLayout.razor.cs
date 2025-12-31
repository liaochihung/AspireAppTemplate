using AspireAppTemplate.Web.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        private bool _drawerOpen;
        private bool _isDarkMode;
        private bool _isBoxed;

        [CascadingParameter]
        public BaseLayout? ParentLayout { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var prefs = await LayoutService.GetPreferenceAsync();
            _drawerOpen = prefs.IsDrawerOpen;
            _isDarkMode = prefs.IsDarkMode;
            _isBoxed = prefs.IsBoxed;

            LayoutService.MajorUpdateOccurred += OnMajorUpdateOccurred;

            // Sync user to local DB
            try 
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    await IdentityApiClient.SyncUserAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to sync user: {ex.Message}");
            }
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
            if (LayoutService.UserPreferences != null)
            {
                _isBoxed = LayoutService.UserPreferences.IsBoxed;
                _isDarkMode = LayoutService.UserPreferences.IsDarkMode; // Ensure this is synced too if reset happens
                _drawerOpen = LayoutService.UserPreferences.IsDrawerOpen;
            }
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            LayoutService.MajorUpdateOccurred -= OnMajorUpdateOccurred;
        }
    }
}
