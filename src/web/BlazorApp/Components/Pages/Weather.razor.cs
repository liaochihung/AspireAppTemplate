using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class Weather
    {
        private WeatherForecast[]? forecasts;

        protected override async Task OnInitializedAsync()
        {
            forecasts = await WeatherApi.GetWeatherAsync();
        }
    }
}
