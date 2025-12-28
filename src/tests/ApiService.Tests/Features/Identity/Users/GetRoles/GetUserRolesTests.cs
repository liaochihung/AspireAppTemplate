using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.GetRoles;
using AspireAppTemplate.Shared;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.GetRoles;

public class GetUserRolesTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetUserRoles_ReturnsRoles_WhenUserExists()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        var userId = "user-id";
        var mockRolesResponse = new[]
        {
            new { Id = "1", Name = "Admin", Description = "Administrator" }
        };
        fakeKeycloak.SetupGetUserRoles(userId, HttpStatusCode.OK, mockRolesResponse);

        var client = fixture.WithMockKeycloak(fakeKeycloak).CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/users/{userId}/roles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedRoles = await response.Content.ReadFromJsonAsync<IEnumerable<KeycloakRole>>();
        returnedRoles.Should().NotBeNull();
        returnedRoles!.Should().HaveCount(1);
    }
}
