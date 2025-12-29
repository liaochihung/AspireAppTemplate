using AspireAppTemplate.Web.Infrastructure.Models;
using Blazored.LocalStorage;

namespace AspireAppTemplate.Web.Infrastructure.Services;

/// <summary>
/// 通知服務，負責管理通知狀態與 LocalStorage 持久化
/// </summary>
public class NotificationService
{
    private const string StorageKey = "aspire-notifications";
    private const int MaxNotifications = 20;
    
    private readonly ILocalStorageService _localStorage;
    private List<NotificationModel> _notifications = new();
    
    /// <summary>
    /// 通知狀態變更事件
    /// </summary>
    public event Action? OnChange;

    public NotificationService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <summary>
    /// 初始化服務，從 LocalStorage 載入通知
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var stored = await _localStorage.GetItemAsync<List<NotificationModel>>(StorageKey);
            _notifications = stored ?? new List<NotificationModel>();
        }
        catch
        {
            _notifications = new List<NotificationModel>();
        }
    }

    /// <summary>
    /// 新增通知
    /// </summary>
    public async Task AddNotificationAsync(string message, string severity)
    {
        var notification = new NotificationModel
        {
            Message = message,
            Severity = severity,
            Timestamp = DateTime.UtcNow
        };

        _notifications.Insert(0, notification); // 新訊息插入最前面

        // FIFO 淘汰：超過最大數量時移除最舊的
        if (_notifications.Count > MaxNotifications)
        {
            _notifications = _notifications.Take(MaxNotifications).ToList();
        }

        await SaveToStorageAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// 標記單筆通知為已讀
    /// </summary>
    public async Task MarkAsReadAsync(Guid id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            await SaveToStorageAsync();
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// 標記全部為已讀
    /// </summary>
    public async Task MarkAllAsReadAsync()
    {
        var hasUnread = _notifications.Any(n => !n.IsRead);
        if (hasUnread)
        {
            foreach (var notification in _notifications)
            {
                notification.IsRead = true;
            }
            await SaveToStorageAsync();
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// 清空所有通知
    /// </summary>
    public async Task ClearAllAsync()
    {
        _notifications.Clear();
        await SaveToStorageAsync();
        NotifyStateChanged();
    }

    /// <summary>
    /// 取得所有通知
    /// </summary>
    public List<NotificationModel> GetNotifications() => _notifications;

    /// <summary>
    /// 取得未讀數量
    /// </summary>
    public int GetUnreadCount() => _notifications.Count(n => !n.IsRead);

    private async Task SaveToStorageAsync()
    {
        try
        {
            await _localStorage.SetItemAsync(StorageKey, _notifications);
        }
        catch
        {
            // LocalStorage 寫入失敗時靜默處理，避免影響使用者體驗
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
