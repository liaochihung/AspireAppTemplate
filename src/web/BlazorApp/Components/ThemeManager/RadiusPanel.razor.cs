using Microsoft.AspNetCore.Components;

namespace AspireAppTemplate.Web.Components.ThemeManager
{
    public partial class RadiusPanel
    {
        [Parameter]
        public double Radius { get; set; }

        [Parameter]
        public int MaxValue { get; set; } = 24;

        [Parameter]
        public EventCallback<double> OnSliderChanged { get; set; }

        private async Task ChangedSelection(ChangeEventArgs e)
        {
            if (double.TryParse(e.Value?.ToString(), out var value))
            {
                await OnSliderChanged.InvokeAsync(value);
            }
        }
    }
}
