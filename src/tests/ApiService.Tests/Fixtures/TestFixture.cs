using AspireAppTemplate.ApiService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        
        Environment.SetEnvironmentVariable("ConnectionStrings__aspiredb", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__cache", _redisContainer.GetConnectionString());
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
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
                { "ConnectionStrings:cache", _redisContainer.GetConnectionString() }
            });
        });

        // Register fake authentication configuration
        builder.ConfigureTestServices(services =>
        {
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
