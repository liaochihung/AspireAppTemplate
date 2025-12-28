using System.Net;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Features.Products.Create;
using AspireAppTemplate.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AspireAppTemplate.ApiService.Tests.Features.Products.Create;

public class CreateProductTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CreateProduct_SavesToDatabase_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateProductRequest 
        { 
            Name = "Integration Test Product", 
            Price = 99.99m,
            Description = "Created by integration test"
        };
        
        var client = fixture.CreateClient();
        
        // Admin Token
        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var testResult = await client.POSTAsync<Endpoint, CreateProductRequest, Product>(request);

        // Assert
        testResult.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        testResult.Result.Should().NotBeNull();
        testResult.Result!.Name.Should().Be(request.Name);

        // Verify DB persistence
        using var scope = fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var saved = db.Products.FirstOrDefault(p => p.Name == request.Name);
        saved.Should().NotBeNull();
        saved!.Price.Should().Be(request.Price);
    }
}
