using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.Update;
using AspireAppTemplate.Shared;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.Update;

public class UpdateUserTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task UpdateUser_ReturnsSuccess_WhenUserExists()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupUpdateUser("user-id", HttpStatusCode.NoContent);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

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
        var response = await client.PUTAsync<Endpoint, UpdateUserRequest>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Put && 
            req.RequestUri!.PathAndQuery.Contains("/admin/realms/test-realm/users/user-id")).Should().BeTrue();
    }
}
