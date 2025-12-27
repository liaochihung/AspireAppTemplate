using Microsoft.AspNetCore.Components;

namespace AspireAppTemplate.Web.Components.ThemeManager
{
    public partial class DarkModePanel
    {
        private bool _isDarkMode;

        [Inject]
        private AspireAppTemplate.Web.Infrastructure.Services.LayoutService LayoutService { get; set; } = default!;

        [Parameter]
        public EventCallback<bool> OnIconClicked { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var prefs = await LayoutService.GetPreferenceAsync();
            _isDarkMode = prefs.IsDarkMode;
        }

        private async Task ToggleDarkMode()
        {
            _isDarkMode = !_isDarkMode;
            await OnIconClicked.InvokeAsync(_isDarkMode);
        }
    }
}
