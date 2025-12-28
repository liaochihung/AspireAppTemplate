using FastEndpoints;
using FastEndpoints.Swagger;
using AspireAppTemplate.Shared;
using Serilog;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Services;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("aspiredb");
builder.AddRedisOutputCache("cache");

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext(),
    writeToProviders: true);

builder.Services.AddProblemDetails();
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

builder.Services.Configure<KeycloakAdminConfiguration>(builder.Configuration.GetSection("KeycloakAdmin"));

var keycloakEndpoint = builder.Configuration["services:keycloak:http:0"];
Log.Information("Keycloak Endpoint from Aspire: {Endpoint}", keycloakEndpoint);

if (!string.IsNullOrEmpty(keycloakEndpoint))
{
    builder.Configuration["Keycloak:AuthServerUrl"] = keycloakEndpoint;
    builder.Configuration["KeycloakAdmin:AuthServerUrl"] = keycloakEndpoint;
}
else
{
    // Fallback to appsettings value
    keycloakEndpoint = builder.Configuration["Keycloak:AuthServerUrl"];
    Log.Information("Keycloak Endpoint from appsettings: {Endpoint}", keycloakEndpoint);
}

builder.Configuration["Keycloak:RequireHttpsMetadata"] = (!builder.Environment.IsDevelopment()).ToString();

// 1. Authentication (Protecting the API)
builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);

// Fix: Allow issuer mismatch (Internal Docker vs External Localhost)
builder.Services.PostConfigure<JwtBearerOptions>(
    JwtBearerDefaults.AuthenticationScheme,
    options =>
{
    options.TokenValidationParameters.ValidateIssuer = false; // Internal service sees 'keycloak:8080' but token issued by 'localhost:port'
    options.TokenValidationParameters.RoleClaimType = "role"; // Align with Keycloak's default short name claim
    options.MapInboundClaims = false; // Prevent mapping 'role' to 'http://.../role'
    
    // Debug: Log token validation events
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Log.Error(context.Exception, "JWT Authentication Failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
            Log.Information("JWT Token Validated. RoleClaimType: {RoleClaimType}. Claims: {Claims}", identity?.RoleClaimType, string.Join(", ", claims ?? Array.Empty<string>()));
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var hasToken = !string.IsNullOrEmpty(context.Token);
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            Log.Information("JWT MessageReceived. HasToken: {HasToken}, AuthHeader: {AuthHeader}", hasToken, authHeader?.Substring(0, Math.Min(authHeader?.Length ?? 0, 50)));
            return Task.CompletedTask;
        }
    };
});

// 2. Admin SDK (Managing Keycloak)
// Configure IdentityService with HttpClient and Password Grant Handler
builder.Services.AddTransient<KeycloakPasswordTokenHandler>();

builder.Services.AddHttpClient<IdentityService>(client =>
{
    if (!string.IsNullOrEmpty(keycloakEndpoint))
    {
        client.BaseAddress = new Uri(keycloakEndpoint.TrimEnd('/') + "/");
    }
})
.AddHttpMessageHandler<KeycloakPasswordTokenHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanManageProducts, policy => 
        policy.RequireRole(AppRoles.Administrator));

    options.AddPolicy(AppPolicies.CanManageRoles, policy => 
        policy.RequireRole(AppRoles.Administrator));

    options.AddPolicy(AppPolicies.CanManageUsers, policy => 
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
app.UseOutputCache();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var claims = context.User.Claims.Select(c => $"{c.Type}: {c.Value}");
        Log.Information("User Authenticated. Claims: {Claims}", string.Join(", ", claims));
    }
    else
    {
        Log.Warning("User NOT Authenticated");
    }
    await next();
});

app.UseFastEndpoints(c => { c.Endpoints.RoutePrefix = "api"; });

if (app.Environment.IsDevelopment()) 
{ 
    app.UseSwaggerGen();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Aspire App API")
               .WithTheme(ScalarTheme.Moon)
               .WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}



app.MapDefaultEndpoints();

try 
{ 
    Log.Information("Starting AspireAppTemplate.ApiService"); 
    await app.RunAsync(); 
}
catch (Exception ex) 
{ 
    Log.Fatal(ex, "AspireAppTemplate.ApiService terminated unexpectedly"); 
}
finally 
{ 
    Log.Information("AspireAppTemplate.ApiService is shutting down"); 
    await Log.CloseAndFlushAsync(); 
}
