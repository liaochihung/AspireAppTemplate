using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.Create;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.Create;

public class CreateUserTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenDataIsValid()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.CreateUserAsync(Arg.Any<KeycloakUser>())
            .Returns(Task.FromResult<ErrorOr<Created>>(Result.Created));

        var client = fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockService);
            });
        }).CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new CreateUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "password123"
        };

        // Act
        var result = await client.POSTAsync<Endpoint, CreateUserRequest, Created>(request);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.Created);
        await mockService.Received(1).CreateUserAsync(Arg.Is<KeycloakUser>(u => u.Username == request.Username));
    }
}
