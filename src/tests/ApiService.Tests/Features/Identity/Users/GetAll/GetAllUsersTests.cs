using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.GetAll;
using AspireAppTemplate.Shared;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.GetAll;

public class GetAllUsersTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetAllUsers_ReturnsUsers_WhenUsersExist()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        var mockUsersResponse = new[]
        {
            new { Id = "1", Username = "user1", Email = "user1@test.com" },
            new { Id = "2", Username = "user2", Email = "user2@test.com" }
        };
        fakeKeycloak.SetupGetAllUsers(HttpStatusCode.OK, mockUsersResponse);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var testResult = await client.GETAsync<Endpoint, PaginationRequest, PaginatedResult<KeycloakUser>>(new PaginationRequest());

        // Assert
        testResult.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        testResult.Result.Should().NotBeNull();
        testResult.Result!.Items.Should().HaveCount(2);
    }
}
