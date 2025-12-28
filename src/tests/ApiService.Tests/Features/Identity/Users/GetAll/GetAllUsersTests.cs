using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.GetAll;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.GetAll;

public class GetAllUsersTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetAllUsers_ReturnsUsers_WhenUsersExist()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        var users = new List<KeycloakUser>
        {
            new() { Id = "1", Username = "user1" },
            new() { Id = "2", Username = "user2" }
        };
        mockService.GetUsersAsync(Arg.Any<string?>())
            .Returns(Task.FromResult<ErrorOr<IEnumerable<KeycloakUser>>>(users));

        var client = fixture.WithWebHostBuilder(b =>
        {
            b.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => mockService);
            });
        }).CreateClient();

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
