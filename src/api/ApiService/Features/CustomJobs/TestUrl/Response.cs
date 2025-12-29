namespace AspireAppTemplate.ApiService.Features.CustomJobs.TestUrl;

public class Response
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public long LatencyMs { get; set; }
    public string? ErrorMessage { get; set; }
}
