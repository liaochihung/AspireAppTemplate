using System.Net;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.Delete;

public class DeleteUserTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task DeleteUser_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        var userId = "test-user-id";
        fakeKeycloak.SetupDeleteUser(userId, HttpStatusCode.NoContent);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Delete && 
            req.RequestUri!.PathAndQuery.Contains($"/admin/realms/test-realm/users/{userId}")).Should().BeTrue();
    }
}
