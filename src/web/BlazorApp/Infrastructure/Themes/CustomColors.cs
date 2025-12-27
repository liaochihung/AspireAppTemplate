using MudBlazor;
using System.Collections.ObjectModel;

namespace AspireAppTemplate.Web.Infrastructure.Themes;

public static class CustomColors
{
    public static IReadOnlyList<string> ThemeColors { get; } = new ReadOnlyCollection<string>(new List<string>
    {
        Light.Primary,
        Colors.Blue.Default,
        Colors.Purple.Default,
        Colors.Orange.Default,
        Colors.Red.Default,
        Colors.Amber.Default,
        Colors.DeepPurple.Default,
        Colors.Pink.Default,
        Colors.Indigo.Default,
        Colors.LightBlue.Default,
        Colors.Cyan.Default,
        Colors.Green.Default,
    });

    public static class Light
    {
        public const string Primary = "#409EFF";
        public const string Secondary = "#67C23A";
        public const string Background = "#F5F7FA";
        public const string AppbarBackground = "#FFFFFF";
        public const string AppbarText = "#303133";
    }

    public static class Dark
    {
        public const string Primary = "#409EFF";
        public const string Secondary = "#67C23A";
        public const string Background = "#121212";
        public const string AppbarBackground = "#1E1E1E";
        public const string DrawerBackground = "#1E1E1E";
        public const string Surface = "#1E1E1E";
        public const string Disabled = "#545454";
    }
}
