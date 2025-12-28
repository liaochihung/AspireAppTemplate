using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.RemoveRole;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.RemoveRole;

public class RemoveRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task RemoveRole_ReturnsSuccess_WhenUserAndRoleExist()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        var userId = "user-id";
        fakeKeycloak.SetupRemoveRole(userId, HttpStatusCode.NoContent);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var roleName = "role-name";

        // Act
        var response = await client.DeleteAsync($"/api/users/{userId}/roles/{roleName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Delete && 
            req.RequestUri!.PathAndQuery.Contains($"/admin/realms/test-realm/users/{userId}/role-mappings/realm")).Should().BeTrue();
    }
}
