using Microsoft.AspNetCore.Components;

namespace AspireAppTemplate.Web.Components.Common;

/// <summary>
/// A search text field with built-in debounce functionality.
/// </summary>
public partial class DebouncedSearchField : ComponentBase, IDisposable
{
    private string _internalValue = string.Empty;
    private System.Timers.Timer? _debounceTimer;

    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = "搜尋...";

    [Parameter]
    public int DebounceMs { get; set; } = 300;

    [Parameter]
    public string Style { get; set; } = "max-width: 300px;";

    [Parameter]
    public string Class { get; set; } = "mt-0";

    protected override void OnParametersSet()
    {
        _internalValue = Value;
    }

    private void OnInput(string value)
    {
        _internalValue = value;

        // Reset timer
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new System.Timers.Timer(DebounceMs);
        _debounceTimer.Elapsed += async (_, _) =>
        {
            _debounceTimer?.Stop();
            await InvokeAsync(async () =>
            {
                await ValueChanged.InvokeAsync(_internalValue);
                StateHasChanged();
            });
        };
        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
    }

    public void Dispose()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
    }
}
