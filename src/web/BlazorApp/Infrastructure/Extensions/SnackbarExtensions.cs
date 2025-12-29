using MudBlazor;
using AspireAppTemplate.Web.Infrastructure.Services;

namespace AspireAppTemplate.Web.Infrastructure.Extensions;

/// <summary>
/// ISnackbar 擴充方法，自動將 Warning/Error 訊息同步到 Notification Center
/// </summary>
public static class SnackbarExtensions
{
    /// <summary>
    /// 顯示 Snackbar 並自動將 Warning/Error 訊息加入 Notification Center
    /// </summary>
    public static async Task AddWithNotificationAsync(
        this ISnackbar snackbar, 
        NotificationService notificationService,
        string message, 
        Severity severity)
    {
        // 顯示 Snackbar
        snackbar.Add(message, severity);

        // 只有 Warning 和 Error 才進入 Notification Center
        if (severity == Severity.Warning || severity == Severity.Error)
        {
            await notificationService.AddNotificationAsync(message, severity.ToString());
        }
    }
}
