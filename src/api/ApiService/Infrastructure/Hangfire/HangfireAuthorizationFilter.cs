using Hangfire.Dashboard;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Infrastructure.Hangfire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;

    public HangfireAuthorizationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // 開發環境允許本地訪問
        if (_environment.IsDevelopment())
        {
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
            if (remoteIp == "127.0.0.1" || remoteIp == "::1" || remoteIp?.StartsWith("192.168.") == true)
            {
                return true;
            }
        }
        
        // 生產環境僅允許 Administrator 角色存取 Dashboard
        return httpContext.User.IsInRole(AppRoles.Administrator);
    }
}
