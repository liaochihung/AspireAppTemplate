using System.Net.Http.Json;

namespace AspireAppTemplate.Web;

public class CustomJobsApiClient(HttpClient httpClient)
{
    public async Task<GetAllJobsResponse> GetAllAsync(CancellationToken ct = default)
    {
        var result = await httpClient.GetFromJsonAsync<GetAllJobsResponse>("/api/custom-jobs", ct);
        return result ?? new GetAllJobsResponse();
    }

    public async Task<CreateJobResponse> CreateAsync(CreateJobRequest request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/custom-jobs", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateJobResponse>(cancellationToken: ct) 
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    public async Task ToggleAsync(Guid id, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync($"/api/custom-jobs/{id}/toggle", null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/custom-jobs/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    // DTOs
    public class GetAllJobsResponse
    {
        public List<JobDto> Jobs { get; set; } = new();
    }

    public class JobDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? CronExpression { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int HttpMethod { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateJobRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? CronExpression { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int HttpMethod { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Headers { get; set; }
        public string? Body { get; set; }
    }

    public class CreateJobResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Type { get; set; }
        public string? CronExpression { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public int HttpMethod { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
