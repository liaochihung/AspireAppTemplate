using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Roles.Delete;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Roles.Delete;

public class DeleteRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task DeleteRole_ReturnsOk_WhenRoleExists()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupDeleteRole("test-role", HttpStatusCode.NoContent);

        var client = fixture.WithMockKeycloak(fakeKeycloak).CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new DeleteRoleRequest { Name = "test-role" };
        var response = await client.DELETEAsync<Endpoint, DeleteRoleRequest>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify request was sent to Keycloak
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Delete && 
            req.RequestUri!.PathAndQuery.Contains("/admin/realms/test-realm/roles/test-role")).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupDeleteRole("missing-role", HttpStatusCode.NotFound);

        var client = fixture.WithMockKeycloak(fakeKeycloak).CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!", 
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var request = new DeleteRoleRequest { Name = "missing-role" };
        var response = await client.DELETEAsync<Endpoint, DeleteRoleRequest>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
