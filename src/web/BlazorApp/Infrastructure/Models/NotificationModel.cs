namespace AspireAppTemplate.Web.Infrastructure.Models;

/// <summary>
/// 通知資料模型
/// </summary>
public class NotificationModel
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 訊息內容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 嚴重程度 (Info, Success, Warning, Error)
    /// </summary>
    public string Severity { get; set; } = "Info";

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已讀
    /// </summary>
    public bool IsRead { get; set; } = false;
}
