using AspireAppTemplate.Web.Infrastructure.Settings;
using AspireAppTemplate.Web.Infrastructure.Themes;
using Microsoft.AspNetCore.Components;

namespace AspireAppTemplate.Web.Components.ThemeManager;

public partial class ThemeDrawer
{
    private bool _open;

    [Parameter]
    public bool ThemeDrawerOpen { get; set; }

    [Parameter]
    public EventCallback<bool> ThemeDrawerOpenChanged { get; set; }

    [EditorRequired]
    [Parameter]
    public UserPreferences? ThemePreference { get; set; }

    [EditorRequired]
    [Parameter]
    public EventCallback<UserPreferences> ThemePreferenceChanged { get; set; }

    private readonly IReadOnlyList<string> _colors = CustomColors.ThemeColors;

    protected override void OnParametersSet()
    {
        _open = ThemeDrawerOpen;
    }

    private async Task CloseDrawer()
    {
        _open = false;
        await ThemeDrawerOpenChanged.InvokeAsync(false);
    }

    private async Task OnOpenChanged(bool open)
    {
        _open = open;
        await ThemeDrawerOpenChanged.InvokeAsync(open);
    }

    private async Task UpdateThemePrimaryColor(string color)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.PrimaryColor = color;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task UpdateThemeSecondaryColor(string color)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.SecondaryColor = color;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task UpdateBorderRadius(double radius)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.BorderRadius = radius;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleDarkLightMode(bool isDarkMode)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.IsDarkMode = isDarkMode;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableDense(bool isDense)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsDense = isDense;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableStriped(bool isStriped)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsStriped = isStriped;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableBorder(bool hasBorder)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.HasBorder = hasBorder;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ToggleEntityTableHoverable(bool isHoverable)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.TablePreference.IsHoverable = isHoverable;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }
    private async Task ToggleBoxedMode(bool isBoxed)
    {
        if (ThemePreference is not null)
        {
            ThemePreference.IsBoxed = isBoxed;
            await ThemePreferenceChanged.InvokeAsync(ThemePreference);
        }
    }

    private async Task ResetToDefault()
    {
        ThemePreference = new UserPreferences();
        await ThemePreferenceChanged.InvokeAsync(ThemePreference);
    }
}
