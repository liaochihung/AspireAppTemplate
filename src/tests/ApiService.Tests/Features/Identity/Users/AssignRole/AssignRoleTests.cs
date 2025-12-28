using System.Net;
using AspireAppTemplate.ApiService.Features.Identity.Users.AssignRole;
using AspireAppTemplate.ApiService.Services;
using AspireAppTemplate.Shared;
using ErrorOr;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AspireAppTemplate.ApiService.Tests.Features.Identity.Users.AssignRole;

public class AssignRoleTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    [Fact]
    public async Task AssignRole_ReturnsSuccess_WhenUserAndRoleExist()
    {
        // Arrange
        var mockService = Substitute.For<IIdentityService>();
        mockService.AssignRoleToUserAsync(Arg.Any<string>(), Arg.Any<string>())
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

        var request = new UserRoleRequest
        {
            Id = "user-id",
            RoleName = "role-name"
        };

        // Act
        // POST /api/users/{id}/roles
        // NOTE: The Endpoint binds FromBody to RoleName, but FromRoute to Id. 
        // Need to be careful with FastEndpoints POST request wrapper if mixed sources.
        // Actually Endpoint<UserRoleRequest> usually expects body.
        // Let's check Endpoint definition locally or usage.
        
        // Use PostAsJsonAsync explicitly to match route and body
        var response = await client.PostAsJsonAsync($"/api/users/{request.Id}/roles", new { roleName = request.RoleName });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await mockService.Received(1).AssignRoleToUserAsync(request.Id, request.RoleName);
    }
}
