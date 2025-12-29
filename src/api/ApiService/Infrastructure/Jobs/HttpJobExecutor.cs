using Hangfire;
using Hangfire.Server;

namespace AspireAppTemplate.ApiService.Infrastructure.Jobs;

/// <summary>
/// 執行自訂 HTTP 任務的核心邏輯
/// </summary>
public class HttpJobExecutor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HttpJobExecutor> _logger;

    public HttpJobExecutor(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<HttpJobExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 執行自訂任務
    /// </summary>
    /// <param name="customJobId">自訂任務 ID</param>
    /// <param name="context">Hangfire 執行上下文（用於顯示進度）</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task ExecuteAsync(Guid customJobId, PerformContext? context = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();

        var job = await db.CustomJobs.FindAsync(customJobId);
        if (job == null)
        {
            _logger.LogWarning("CustomJob {JobId} not found, skipping execution", customJobId);
            return;
        }

        _logger.LogInformation(
            "Executing CustomJob {JobId} ({JobName}): {Method} {Url}",
            job.Id, job.Name, job.HttpMethod, job.Url);



        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(
            new System.Net.Http.HttpMethod(job.HttpMethod.ToString()),
            job.Url
        );

        // 設定 Headers
        if (!string.IsNullOrEmpty(job.Headers))
        {
            var headers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(job.Headers);
            if (headers != null)
            {
                foreach (var (key, value) in headers)
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }

        // 設定 Body
        if (!string.IsNullOrEmpty(job.Body))
        {
            request.Content = new StringContent(
                job.Body,
                System.Text.Encoding.UTF8,
                "application/json"
            );
        }

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation(
            "CustomJob {JobId} executed successfully. Status: {StatusCode}, Response: {Response}",
            job.Id, response.StatusCode, responseBody.Length > 200 ? responseBody[..200] + "..." : responseBody);
    }
}
