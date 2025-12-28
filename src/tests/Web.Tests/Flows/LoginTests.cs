using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AspireAppTemplate.Web.Tests.Flows;

[TestClass]
public class LoginTests : PageTest
{
    private static DistributedApplication? _app;
    private static string? _frontendUrl;

    [ClassInitialize]
    public static async Task ClassSetup(TestContext context)
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireAppTemplate_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // Wait for webfrontend to be ready
        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService.WaitForResourceHealthyAsync("webfrontend", CancellationToken.None);

        _frontendUrl = _app.GetEndpoint("webfrontend")?.Url.ToString();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task LoginFlow_RedirectsToKeycloakAndBack_WhenCredentialsAreValid()
    {
        // 1. Navigate to Home
        await Page.GotoAsync(_frontendUrl ?? throw new InvalidOperationException("Frontend URL not found"));
        await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

        // 2. Click Login (assuming there is a Login button/link)
        // If auto-login is enabled (from previous context?), it might redirect automatically.
        // Let's assume we need to click "Login" in the header if not authenticated.
        var loginButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Login" });
        
        // If we are already on Keycloak page (Auto-Redirect), we might not find Login button.
        // Check if we are on Keycloak
        if (await Page.TitleAsync() == "Sign in to WeatherShop")
        {
             // Already redirected
        }
        else if (await loginButton.IsVisibleAsync())
        {
            await loginButton.ClickAsync();
        }
        
        // 3. Verify Keycloak Page
        // Wait for username input
        await Page.WaitForSelectorAsync("#username");
        
        // 4. Fill Credentials
        await Page.FillAsync("#username", "admin");
        await Page.FillAsync("#password", "admin");
        await Page.ClickAsync("#kc-login");

        // 5. Verify Return to App
        await Page.WaitForURLAsync(url => url.Contains("localhost")); // Back to App
        
        // Verify Logged In State (e.g., "Logout" button visible or User Avatar)
        // Adjust selector based on actual UI
        var logoutButton = Page.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Logout" });
        // Or check text "Hello, admin"?
        
        await Expect(Page.Locator("text=admin")).ToBeVisibleAsync(); // Assuming username is displayed
    }
}
