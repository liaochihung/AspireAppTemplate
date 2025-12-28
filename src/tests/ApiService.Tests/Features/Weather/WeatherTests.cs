using System.Net;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Tests.Features.Weather;

public class WeatherTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetWeather_ReturnsOk_AndForecasts()
    {
        // Arrange
        var client = fixture.CreateClient();
        // Weather endpoint allows anonymous usually, or check Endpoint.cs

        // Act
        // Assuming GET /weather
        var response = await client.GetAsync("/api/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var forecasts = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();
        forecasts.Should().NotBeNull();
        forecasts!.Should().HaveCount(5);
    }
}
