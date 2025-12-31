using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspireAppTemplate.Web;
using AspireAppTemplate.Web.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using AspireAppTemplate.Shared;
using MudBlazor.Services;
using Blazored.LocalStorage;
using AspireAppTemplate.Web.Infrastructure.Services;
using AspireAppTemplate.Web.Services;


var builder = WebApplication.CreateBuilder(args);

// Clear default logging providers (like Console) to avoid duplicate logs,
// but keep Serilog flowing to OpenTelemetry via writeToProviders: true.
builder.Logging.ClearProviders();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext(),
    writeToProviders: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add memory cache for token caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<AspireAppTemplate.Web.Infrastructure.Authentication.TokenCacheService>();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<NotificationService>();


builder.Services.AddHttpContextAccessor()
                .AddTransient<AuthorizationHandler>();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    }).AddHttpMessageHandler<AuthorizationHandler>();


builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
    // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
    client.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddHttpClient<IdentityApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddHttpClient<CustomJobsApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddHttpClient<AuditLogApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<AuthorizationHandler>();


builder.Services.AddHttpClient<StorageApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
}).AddHttpMessageHandler<AuthorizationHandler>();



var oidcScheme = OpenIdConnectDefaults.AuthenticationScheme;
builder.Services.AddAuthentication(oidcScheme)
    .AddKeycloakOpenIdConnect("keycloak", realm: "AspireApp", oidcScheme, options =>
    {
        options.ClientId="WebApp";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("api:all");
        options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
        options.TokenValidationParameters.RoleClaimType = "role"; // Keycloak uses short "role" claim
        options.SaveTokens = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.MapInboundClaims = false; // Prevent mapping 'role' to long URI format

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
    }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanManageProducts, policy => 
        policy.RequireRole(AppRoles.Administrator));

    options.AddPolicy(AppPolicies.CanViewWeather, policy => 
        policy.RequireRole(AppRoles.Administrator));
    
    options.AddPolicy(AppPolicies.CanManageRoles, policy => 
        policy.RequireRole(AppRoles.Administrator));
    
    options.AddPolicy(AppPolicies.CanManageUsers, policy => 
        policy.RequireRole(AppRoles.Administrator));
    
    options.AddPolicy(AppPolicies.CanManageSystem, policy => 
        policy.RequireRole(AppRoles.Administrator));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

// Persist cached tokens to cookies (must run before response starts)
app.UseMiddleware<AspireAppTemplate.Web.Infrastructure.Middleware.TokenPersistenceMiddleware>();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.MapLoginAndLogout();

try
{
    Log.Information("Starting AspireAppTemplate.Web");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AspireAppTemplate.Web terminated unexpectedly");
}
finally
{
    Log.Information("AspireAppTemplate.Web is shutting down");
    await Log.CloseAndFlushAsync();
}
