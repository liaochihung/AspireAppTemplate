using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using AspireAppTemplate.Shared;
using ErrorOr;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;

namespace AspireAppTemplate.ApiService.Features.Weather.GetWeatherForecast;

public class Endpoint : EndpointWithoutRequest<IEnumerable<WeatherForecast>>
{
    public override void Configure()
    {
        Get("/weatherforecast");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)), Random.Shared.Next(-20, 55), summaries[Random.Shared.Next(summaries.Length)]))
            .ToArray();

        // Wrap in ErrorOr and send result
        ErrorOr<IEnumerable<WeatherForecast>> result = forecast.ToList();
        await this.SendResultAsync(result, ct: ct);
    }
}
