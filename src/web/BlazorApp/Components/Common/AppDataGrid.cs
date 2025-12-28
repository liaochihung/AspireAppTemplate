using AspireAppTemplate.Web.Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Common;

/// <summary>
/// A customized MudDataGrid that automatically applies user table preferences
/// (Dense, Striped, Bordered, Hover) from LayoutService and reacts to theme changes.
/// </summary>
public class AppDataGrid<T> : MudDataGrid<T>
{
    [Inject]
    private LayoutService LayoutService { get; set; } = default!;

    private bool _preferencesApplied;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to theme updates
        LayoutService.MajorUpdateOccurred += OnThemeUpdated;
        
        // Apply initial preferences synchronously from cached values
        ApplyTablePreferencesSync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender && !_preferencesApplied)
        {
            _preferencesApplied = true;
            await ApplyTablePreferencesAsync();
            StateHasChanged();
        }
    }

    private void OnThemeUpdated(object? sender, EventArgs e)
    {
        InvokeAsync(async () =>
        {
            await ApplyTablePreferencesAsync();
            StateHasChanged();
        });
    }

    private void ApplyTablePreferencesSync()
    {
        // Use default values initially - will be updated in OnAfterRenderAsync
        var prefs = LayoutService.UserPreferences;
        if (prefs != null)
        {
            var tablePref = prefs.TablePreference;
            Dense = tablePref.IsDense;
            Striped = tablePref.IsStriped;
            Bordered = tablePref.HasBorder;
            Hover = tablePref.IsHoverable;
        }
    }

    private async Task ApplyTablePreferencesAsync()
    {
        var prefs = await LayoutService.GetPreferenceAsync();
        var tablePref = prefs.TablePreference;
        
        Dense = tablePref.IsDense;
        Striped = tablePref.IsStriped;
        Bordered = tablePref.HasBorder;
        Hover = tablePref.IsHoverable;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            LayoutService.MajorUpdateOccurred -= OnThemeUpdated;
        }
        base.Dispose(disposing);
    }
}
