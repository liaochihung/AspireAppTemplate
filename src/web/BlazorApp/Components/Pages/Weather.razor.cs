using AspireAppTemplate.Shared;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages;

public partial class Weather
{
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        forecasts = await WeatherApi.GetWeatherAsync();
    }

    private static Color GetTemperatureColor(int temp) => temp switch
    {
        > 30 => Color.Error,
        > 20 => Color.Warning,
        > 10 => Color.Success,
        _ => Color.Info
    };

    private static string GetTemperatureIcon(int temp) => temp switch
    {
        > 25 => Icons.Material.Filled.WbSunny,
        > 15 => Icons.Material.Filled.WbCloudy,
        _ => Icons.Material.Filled.AcUnit
    };
}
