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

    private static Color GetTemperatureColor(int temp) =>
        temp > 30 ? Color.Error :
        temp > 20 ? Color.Warning :
        temp > 10 ? Color.Success : Color.Info;

    private static string GetTemperatureIcon(int temp) =>
        temp > 25 ? Icons.Material.Filled.WbSunny :
        temp > 15 ? Icons.Material.Filled.WbCloudy : Icons.Material.Filled.AcUnit;
}
