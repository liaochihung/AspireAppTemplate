using AspireAppTemplate.Shared;

namespace AspireAppTemplate.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        var forecasts = await httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weatherforecast", cancellationToken);
        return forecasts?.Take(maxItems).ToArray() ?? [];
    }
}
