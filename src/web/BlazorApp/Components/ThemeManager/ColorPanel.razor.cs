using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.ThemeManager
{
    public partial class ColorPanel
    {
        [Parameter]
        public IReadOnlyList<string> Colors { get; set; } = Array.Empty<string>();

        [Parameter]
        public string ColorType { get; set; } = "主色調";

        [Parameter]
        public Color CurrentColor { get; set; } = Color.Primary;

        [Parameter]
        public EventCallback<string> OnColorClicked { get; set; }

        private async Task ColorClicked(string color)
        {
            await OnColorClicked.InvokeAsync(color);
        }
    }
}
