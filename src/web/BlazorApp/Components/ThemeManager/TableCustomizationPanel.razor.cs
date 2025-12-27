using Microsoft.AspNetCore.Components;

namespace AspireAppTemplate.Web.Components.ThemeManager
{
    public partial class TableCustomizationPanel
    {
        [Parameter] public bool IsDense { get; set; }
        [Parameter] public bool IsStriped { get; set; }
        [Parameter] public bool HasBorder { get; set; }
        [Parameter] public bool IsHoverable { get; set; }

        [Parameter] public EventCallback<bool> OnDenseSwitchToggled { get; set; }
        [Parameter] public EventCallback<bool> OnStripedSwitchToggled { get; set; }
        [Parameter] public EventCallback<bool> OnBorderdedSwitchToggled { get; set; }
        [Parameter] public EventCallback<bool> OnHoverableSwitchToggled { get; set; }

        private async Task ToggleDenseSwitch(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value?.ToString(), out var value))
                await OnDenseSwitchToggled.InvokeAsync(value);
        }

        private async Task ToggleStripedSwitch(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value?.ToString(), out var value))
                await OnStripedSwitchToggled.InvokeAsync(value);
        }

        private async Task ToggleBorderedSwitch(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value?.ToString(), out var value))
                await OnBorderdedSwitchToggled.InvokeAsync(value);
        }

        private async Task ToggleHoverableSwitch(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value?.ToString(), out var value))
                await OnHoverableSwitchToggled.InvokeAsync(value);
        }
    }
}
