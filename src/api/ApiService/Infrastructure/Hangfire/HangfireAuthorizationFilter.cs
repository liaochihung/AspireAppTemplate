using Hangfire.Dashboard;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Infrastructure.Hangfire;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // 僅允許 Administrator 角色存取 Dashboard
        return httpContext.User.IsInRole(AppRoles.Administrator);
    }
}
