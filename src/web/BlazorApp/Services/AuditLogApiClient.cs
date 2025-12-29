using System.Net.Http.Json;
using AspireAppTemplate.Shared.Models;

namespace AspireAppTemplate.Web.Services;

public class AuditLogApiClient
{
    private readonly HttpClient _httpClient;

    public AuditLogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _httpClient.GetFromJsonAsync<List<AuditLogDto>>("api/audit-logs", cancellationToken);
        return logs ?? new List<AuditLogDto>();
    }
}
