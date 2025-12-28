using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Roles.Create;
using AspireAppTemplate.Shared;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Roles.Create;

public class CreateRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CreateRole_ReturnsCreated_WhenDataIsValid()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupCreateRole(HttpStatusCode.Created);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var request = new CreateRoleRequest
        {
            Name = "test-role",
            Description = "Test Role Description"
        };
        
        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!", 
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var result = await client.POSTAsync<Endpoint, CreateRoleRequest, KeycloakRole>(request);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify request was sent to Keycloak
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Post && 
            req.RequestUri!.PathAndQuery.Contains("/admin/realms/test-realm/roles")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateRole_ReturnsConflict_WhenRoleAlreadyExists()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupCreateRole(HttpStatusCode.Conflict);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var request = new CreateRoleRequest { Name = "existing-role" };
        
        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!", 
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var result = await client.POSTAsync<Endpoint, CreateRoleRequest, EmptyResponse>(request);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
