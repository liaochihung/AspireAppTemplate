using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Roles.Delete;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Roles.Delete;

public class DeleteRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task DeleteRole_ReturnsOk_WhenRoleExists()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.DeleteRoleAsync(Arg.Any<string>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(Result.Deleted));

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
        var request = new DeleteRoleRequest { Name = "test-role" };
        var response = await client.DELETEAsync<Endpoint, DeleteRoleRequest>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await mockService.Received(1).DeleteRoleAsync("test-role");
    }

    [Fact]
    public async Task DeleteRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.DeleteRoleAsync(Arg.Any<string>())
            .Returns(Task.FromResult<ErrorOr<Deleted>>(Error.NotFound()));

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
        var request = new DeleteRoleRequest { Name = "missing-role" };
        var response = await client.DELETEAsync<Endpoint, DeleteRoleRequest>(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
