using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Roles.GetAll;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Keycloak.AuthServices.Sdk.Admin.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Roles.GetAll;

public class GetAllRolesTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task GetAllRoles_ReturnsRoles_WhenRolesExist()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        var roles = new List<KeycloakRole>
        {
            new() { Id = "1", Name = "Admin", Description = "Administrator" },
            new() { Id = "2", Name = "User", Description = "User" }
        };
        mockService.GetRolesAsync()
            .Returns(Task.FromResult<ErrorOr<IEnumerable<KeycloakRole>>>(roles));

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
        var testResult = await client.GETAsync<Endpoint, PaginationRequest, PaginatedResult<KeycloakRole>>(new PaginationRequest());

        // Assert
        testResult.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        testResult.Result.Should().NotBeNull();
        testResult.Result!.Items.Should().HaveCount(2);
        testResult.Result.Items.First().Name.Should().Be("Admin");
    }
}
