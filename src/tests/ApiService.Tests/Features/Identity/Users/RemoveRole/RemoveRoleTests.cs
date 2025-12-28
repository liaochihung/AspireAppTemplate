using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.RemoveRole;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.RemoveRole;

public class RemoveRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task RemoveRole_ReturnsSuccess_WhenUserAndRoleExist()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.RemoveRoleFromUserAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult<ErrorOr<Success>>(Result.Success));

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

        var userId = "user-id";
        var roleName = "role-name";

        // Act
        // DELETE /api/users/{id}/roles/{RoleName}
        var response = await client.DeleteAsync($"/api/users/{userId}/roles/{roleName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await mockService.Received(1).RemoveRoleFromUserAsync(userId, roleName);
    }
}
