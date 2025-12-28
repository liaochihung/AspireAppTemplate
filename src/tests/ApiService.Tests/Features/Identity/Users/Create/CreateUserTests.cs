using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.Create;
using AspireAppTemplate.Shared;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.Create;

public class CreateUserTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task CreateUser_ReturnsCreated_WhenDataIsValid()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        fakeKeycloak.SetupCreateUser(HttpStatusCode.Created);

        var client = fixture.WithMockKeycloak(fakeKeycloak).CreateClient();

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
        var result = await client.POSTAsync<Endpoint, CreateUserRequest, EmptyResponse>(request);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.Created);
        fakeKeycloak.VerifyRequestSent(req => 
            req.Method == HttpMethod.Post && 
            req.RequestUri!.PathAndQuery.Contains("/admin/realms/test-realm/users")).Should().BeTrue();
    }
}
