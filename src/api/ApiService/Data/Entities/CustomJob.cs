namespace AspireAppTemplate.ApiService.Data.Entities;

public class CustomJob
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public JobType Type { get; set; }
    
    // 排程設定
    public string? CronExpression { get; set; }      // 週期性任務使用
    public DateTime? ScheduledAt { get; set; }       // 一次性任務使用
    
    // HTTP 設定
    public HttpMethod HttpMethod { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Headers { get; set; }             // JSON 格式: {"Key": "Value"}
    public string? Body { get; set; }                // JSON 字串
    
    // 狀態追蹤
    public bool IsActive { get; set; }
    public string? HangfireJobId { get; set; }       // Hangfire 的 Recurring Job ID
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public enum JobType
{
    OneTime = 1,    // 一次性
    Recurring = 2   // 週期性
}

public enum HttpMethod
{
    GET = 1,
    POST = 2,
    PUT = 3,
    DELETE = 4
}
