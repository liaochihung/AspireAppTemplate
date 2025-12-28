using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Roles.GetAll;
using AspireAppTemplate.Shared;
using Keycloak.AuthServices.Sdk.Admin.Models;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Roles.GetAll;

public class GetAllRolesTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetAllRoles_ReturnsRoles_WhenRolesExist()
    {
        // Arrange
        var fakeKeycloak = new FakeKeycloakHandler();
        var mockRolesResponse = new[]
        {
            new { Id = "1", Name = "Admin", Description = "Administrator" },
            new { Id = "2", Name = "User", Description = "User" }
        };
        fakeKeycloak.SetupGetAllRoles(HttpStatusCode.OK, mockRolesResponse);

        fixture.SetMockKeycloakHandler(fakeKeycloak);
        var client = fixture.CreateClient();

        var token = JWTBearer.CreateToken(
            signingKey: "VerifyTheIntegrityOfThisTokenSignature123!",
            claims: new[] { ("role", AppRoles.Administrator) });

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var testResult = await client.GETAsync<Endpoint, PaginationRequest, PaginatedResult<KeycloakRole>>(new PaginationRequest());

        // Assert
        testResult.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        testResult.Result.Should().NotBeNull();
        testResult.Result!.Items.Should().HaveCount(2);
        testResult.Result.Items.First().Name.Should().Be("Admin");
    }
}
