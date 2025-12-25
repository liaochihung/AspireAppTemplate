using AspireAppTemplate.ApiService;
using FastEndpoints;
using FastEndpoints.Swagger;
using AspireAppTemplate.Shared;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Clear default logging providers (like Console) to avoid duplicate logs,
// but keep Serilog flowing to OpenTelemetry via writeToProviders: true.
builder.Logging.ClearProviders();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

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

            // 建議：將 Keycloak 的 roles 對映到 Claims
            // options.TokenValidationParameters.RoleClaimType = "roles";
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanManageProducts, policy => 
        policy.RequireRole(AppRoles.Administrator));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

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
