using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Roles.Create;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Roles.Create;

public class CreateRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CreateRole_ReturnsCreated_WhenDataIsValid()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.CreateRoleAsync(Arg.Any<KeycloakRole>())
            .Returns(Task.FromResult<ErrorOr<Created>>(Result.Created));

        var client = fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockService);
            });
        }).CreateClient();

        var request = new CreateRoleRequest
        {
            Name = "test-role",
            Description = "Test Role Description"
        };
        
        // Generate a fake token with Admin role
        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!", 
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var result = await client.POSTAsync<Endpoint, CreateRoleRequest, KeycloakRole>(request);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.Created);
        await mockService.Received(1).CreateRoleAsync(Arg.Is<KeycloakRole>(r => r.Name == request.Name));
    }

    [Fact]
    public async Task CreateRole_ReturnsConflict_WhenRoleAlreadyExists()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.CreateRoleAsync(Arg.Any<KeycloakRole>())
            .Returns(Task.FromResult<ErrorOr<Created>>(Error.Conflict("Role already exists.")));

        var client = fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockService);
            });
        }).CreateClient();

        var request = new CreateRoleRequest { Name = "existing-role" };
        
        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!", 
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.PostAsJsonAsync("/api/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
