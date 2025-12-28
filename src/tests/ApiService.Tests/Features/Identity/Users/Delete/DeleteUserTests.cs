using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.Delete;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.Delete;

public class DeleteUserTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task DeleteUser_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.DeleteUserAsync(Arg.Any<string>())
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
        // DELETE /api/users/{id}
        var userId = "test-user-id";
        var response = await client.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await mockService.Received(1).DeleteUserAsync(userId);
    }
}
