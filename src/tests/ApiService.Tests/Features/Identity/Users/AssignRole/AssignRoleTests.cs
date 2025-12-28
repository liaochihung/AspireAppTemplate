using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.AssignRole;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.AssignRole;

public class AssignRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task AssignRole_ReturnsSuccess_WhenUserAndRoleExist()
    {
        // Arrange  
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupAssignRole("user-id", HttpStatusCode.NoContent);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new UserRoleRequest
        {
            Id = "user-id",
            RoleName = "role-name"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/users/{request.Id}/roles", new { roleName = request.RoleName });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Post && 
            req.RequestUri!.PathAndQuery.Contains($"/admin/realms/test-realm/users/{request.Id}/role-mappings/realm")).Should().BeTrue();
    }
}
