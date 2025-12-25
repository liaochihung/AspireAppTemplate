using FastEndpoints;
using FastEndpoints.Swagger;
using AspireAppTemplate.Shared;
using Serilog;
using AspireAppTemplate.Database;

var builder = WebApplication.CreateBuilder(args);

// Clear default logging providers (like Console) to avoid duplicate logs,
// but keep Serilog flowing to OpenTelemetry via writeToProviders: true.
builder.Logging.ClearProviders();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("aspiredb"); // 已內建 Health Check

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext(),
    writeToProviders: true);

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(); // FastEndpoints 的 Swagger 整合

builder.Services.AddAuthentication()
    .AddKeycloakJwtBearer(
        serviceName: "keycloak", 
        realm: "WeatherShop", options =>
        {
            options.Audience = "weather.api";
            if(builder.Environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanManageProducts, policy => 
        policy.RequireRole(AppRoles.Administrator));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";
});

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(); // 使用 FastEndpoints 的 Swagger UI
}

// 保留原本的 Minimal API 範例，或者將其也遷移到 FastEndpoints
app.MapGet("/weatherforecast", () =>
{
    string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization();

app.MapDefaultEndpoints();

try
{
    Log.Information("Starting AspireAppTemplate.ApiService");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AspireAppTemplate.ApiService terminated unexpectedly");
}
finally
{
    Log.Information("AspireAppTemplate.ApiService is shutting down");
    Log.CloseAndFlush();
}
