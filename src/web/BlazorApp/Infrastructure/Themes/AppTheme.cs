using MudBlazor;

namespace AspireAppTemplate.Web.Infrastructure.Themes;

public class AppTheme : MudTheme
{
    public AppTheme()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#409EFF",
            Secondary = "#67C23A",
            Info = "#909399",
            Success = "#67C23A",
            Warning = "#E6A23C",
            Error = "#F56C6C",
            Background = "#F5F7FA",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#303133",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#303133",
            TextPrimary = "#303133",
            TextSecondary = "#606266",
        };

        PaletteDark = new PaletteDark
        {
            Primary = "#409EFF",
            Secondary = "#67C23A",
            Info = "#909399",
            Success = "#67C23A",
            Warning = "#E6A23C",
            Error = "#F56C6C",
            Background = "#121212",
            Surface = "#1E1E1E",
            AppbarBackground = "#1E1E1E",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#1E1E1E",
            DrawerText = "#FFFFFF",
            TextPrimary = "#FFFFFF",
            TextSecondary = "#B0B0B0",
        };

        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "4px",
        };

        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Helvetica Neue", "Helvetica", "PingFang SC", "Microsoft YaHei", "sans-serif" }
            }
        };

    }
}
