using AspireAppTemplate.ApiService.Data.Entities;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.Create;

public class Request
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public JobType Type { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public Data.Entities.HttpMethod HttpMethod { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Headers { get; set; }
    public string? Body { get; set; }
}
