using AspireAppTemplate.ApiService.Data;
using Hangfire;

namespace AspireAppTemplate.ApiService.Infrastructure.Jobs;

public class LogCleanupJob
{
    private readonly ILogger<LogCleanupJob> _logger;

    public LogCleanupJob(ILogger<LogCleanupJob> logger)
    {
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteAsync(int retentionDays, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting log cleanup for records older than {Days} days", retentionDays);

        // TODO: 實作實際的清理邏輯
        // 範例：await _db.AuditLogs.Where(log => log.CreatedAt < cutoffDate).ExecuteDeleteAsync(ct);
        
        await Task.Delay(1000, ct); // 模擬清理作業
        
        _logger.LogInformation("Log cleanup completed successfully");
    }
}
