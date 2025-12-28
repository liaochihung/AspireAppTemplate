using AspireAppTemplate.ApiService;
using AspireAppTemplate.ApiService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace AspireAppTemplate.ApiService.Tests.Fixtures;

public class TestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("aspiredb")
        .WithUsername("postgres")
        .WithPassword("1111")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:alpine")
        .Build();


    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        Environment.SetEnvironmentVariable("ConnectionStrings__aspiredb", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__cache", _redisContainer.GetConnectionString());
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    private FakeKeycloakHandler? _mockKeycloakHandler;

    /// <summary>
    /// 設定 Mock Keycloak Handler
    /// </summary>
    public void SetMockKeycloakHandler(FakeKeycloakHandler handler)
    {
        _mockKeycloakHandler = handler;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Direct setting injection
        builder.UseSetting("ConnectionStrings:aspiredb", _dbContainer.GetConnectionString());
        builder.UseSetting("ConnectionStrings:cache", _redisContainer.GetConnectionString());

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:aspiredb", _dbContainer.GetConnectionString() },
                { "ConnectionStrings:cache", _redisContainer.GetConnectionString() },
                
                // Keycloak Configuration for Testing
                { "Keycloak:AuthServerUrl", "http://test-keycloak:8080" },
                { "Keycloak:Realm", "test-realm" },
                { "Keycloak:Resource", "test-client" },
                { "Keycloak:SslRequired", "none" },
                { "Keycloak:VerifyTokenAudience", "false" },
                
                // Keycloak Admin Configuration for Testing
                { "KeycloakAdmin:AuthServerUrl", "http://test-keycloak:8080" },
                { "KeycloakAdmin:Realm", "master" },
                { "KeycloakAdmin:Resource", "admin-cli" },
                { "KeycloakAdmin:AdminUsername", "admin" },
                { "KeycloakAdmin:AdminPassword", "admin" },
                { "KeycloakAdmin:TargetRealm", "test-realm" }
            });
        });

        // Register fake authentication configuration
        builder.ConfigureTestServices(services =>
        {
            // 註冊一個 Dummy KeycloakPasswordTokenHandler (測試環境不需要真實的 Token Handler)
            // 這樣 Program.cs 中的 .AddHttpMessageHandler<KeycloakPasswordTokenHandler>() 就不會失敗
            services.AddTransient<KeycloakPasswordTokenHandler>(sp => 
            {
                // 返回一個什麼都不做的 Handler
                return new KeycloakPasswordTokenHandler(
                    Microsoft.Extensions.Options.Options.Create(new KeycloakAdminConfiguration()),
                    sp.GetRequiredService<ILogger<KeycloakPasswordTokenHandler>>());
            });
            
            // 如果設定了 Mock Keycloak Handler，則替換 IdentityService 的 HttpClient
            if (_mockKeycloakHandler != null)
            {
                // 移除原有的 IIdentityService HttpClient 註冊
                var descriptors = services.Where(d => 
                    d.ServiceType.IsGenericType && 
                    d.ServiceType.Name.Contains("IHttpClientBuilder")).ToList();
                
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }
                
                // 重新註冊 IdentityService 使用 Mock HttpClient
                services.AddHttpClient<IIdentityService, IdentityService>()
                    .ConfigurePrimaryHttpMessageHandler(() => _mockKeycloakHandler);
            }
            
            services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.TokenValidationParameters.ValidateAudience = false;
                    options.TokenValidationParameters.ValidateLifetime = false;
                    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    options.TokenValidationParameters.IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes("VerifyTheIntegrityOfThisTokenSignature123!"));
                    options.TokenValidationParameters.RequireSignedTokens = true;
                });
        });
    }
}
