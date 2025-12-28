using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.Update;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.Update;

public class UpdateUserTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task UpdateUser_ReturnsSuccess_WhenUserExists()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.UpdateUserAsync(Arg.Any<string>(), Arg.Any<KeycloakUser>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

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

        var request = new UpdateUserRequest
        {
            Id = "user-id",
            Username = "updateduser",
            Email = "updated@example.com",
            FirstName = "Updated",
            LastName = "User",
            Enabled = true
        };

        // Act
        // PUT /api/users/{id}
        var response = await client.PutAsJsonAsync($"/api/users/{request.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await mockService.Received(1).UpdateUserAsync(request.Id, Arg.Is<KeycloakUser>(u => u.Username == request.Username));
    }
}
