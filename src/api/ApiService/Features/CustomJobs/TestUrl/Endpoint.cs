using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AspireAppTemplate.Shared;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.TestUrl;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public Endpoint(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override void Configure()
    {
        Post("/custom-jobs/test-url");
        Policies(AppPolicies.CanManageSystem);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // 建立 HttpClient（10 秒超時）
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            // 建立 HttpRequestMessage
            var method = req.HttpMethod switch
            {
                Data.Entities.HttpMethod.GET => System.Net.Http.HttpMethod.Get,
                Data.Entities.HttpMethod.POST => System.Net.Http.HttpMethod.Post,
                Data.Entities.HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
                Data.Entities.HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
                _ => System.Net.Http.HttpMethod.Get
            };

            var request = new HttpRequestMessage(method, req.Url);

            // 處理 Headers
            if (!string.IsNullOrWhiteSpace(req.Headers))
            {
                try
                {
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(req.Headers);
                    if (headers != null)
                    {
                        foreach (var (key, value) in headers)
                        {
                            request.Headers.TryAddWithoutValidation(key, value);
                        }
                    }
                }
                catch (JsonException)
                {
                    await SendAsync(new Response
                    {
                        IsSuccess = false,
                        ErrorMessage = "Headers 格式錯誤，必須為有效的 JSON 格式"
                    }, cancellation: ct);
                    return;
                }
            }

            // 處理 Body（僅 POST/PUT）
            if (req.HttpMethod is Data.Entities.HttpMethod.POST or Data.Entities.HttpMethod.PUT 
                && !string.IsNullOrWhiteSpace(req.Body))
            {
                request.Content = new StringContent(req.Body, Encoding.UTF8, "application/json");
            }

            // 發送請求
            var response = await client.SendAsync(request, ct);
            stopwatch.Stop();

            // 讀取 Response Body（限制 10KB）
            var responseBody = await response.Content.ReadAsStringAsync(ct);
            if (responseBody.Length > 10_000)
            {
                responseBody = responseBody[..10_000] + "... (truncated)";
            }

            await SendAsync(new Response
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody,
                LatencyMs = stopwatch.ElapsedMilliseconds
            }, cancellation: ct);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            await SendAsync(new Response
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = $"HTTP 請求失敗: {ex.Message}",
                LatencyMs = stopwatch.ElapsedMilliseconds
            }, cancellation: ct);
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            await SendAsync(new Response
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = "請求超時（超過 10 秒）",
                LatencyMs = stopwatch.ElapsedMilliseconds
            }, cancellation: ct);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await SendAsync(new Response
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = $"未預期的錯誤: {ex.Message}",
                LatencyMs = stopwatch.ElapsedMilliseconds
            }, cancellation: ct);
        }
    }
}
