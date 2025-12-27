using FastEndpoints;
using FastEndpoints.Swagger;
using AspireAppTemplate.Shared;
using Serilog;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("aspiredb");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext(),
    writeToProviders: true);

builder.Services.AddProblemDetails();
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

builder.Services.Configure<KeycloakConfiguration>(builder.Configuration.GetSection("Keycloak"));
builder.Services.AddHttpClient<IdentityService>();

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

app.UseFastEndpoints(c => { c.Endpoints.RoutePrefix = "api"; });

if (app.Environment.IsDevelopment()) { app.UseSwaggerGen(); }

app.MapGet("/weatherforecast", () =>
{
    string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];
    return Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)), Random.Shared.Next(-20, 55), summaries[Random.Shared.Next(summaries.Length)]))
        .ToArray();
})
.WithName("GetWeatherForecast")
.RequireAuthorization();

app.MapDefaultEndpoints();

try { Log.Information("Starting AspireAppTemplate.ApiService"); app.Run(); }
catch (Exception ex) { Log.Fatal(ex, "AspireAppTemplate.ApiService terminated unexpectedly"); }
finally { Log.Information("AspireAppTemplate.ApiService is shutting down"); Log.CloseAndFlush(); }
