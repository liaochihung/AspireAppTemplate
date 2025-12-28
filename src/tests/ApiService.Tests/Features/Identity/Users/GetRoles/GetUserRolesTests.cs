using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.GetRoles;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.GetRoles;

public class GetUserRolesTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetUserRoles_ReturnsRoles_WhenUserExists()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        var roles = new List<KeycloakRole>
        {
            new() { Id = "1", Name = "Admin" }
        };
        mockService.GetUserRolesAsync(Arg.Any<string>())
            .Returns(Task.FromResult<ErrorOr<IEnumerable<KeycloakRole>>>(roles));

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

        var userId = "user-id";

        // Act
        // GET /api/users/{id}/roles
        var response = await client.GetAsync($"/api/users/{userId}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedRoles = await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakRole>>();
        returnedRoles.Should().NotBeNull();
        returnedRoles!.Should().HaveCount(1);
    }
}
