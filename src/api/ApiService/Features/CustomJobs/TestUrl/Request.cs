using AspireAppTemplate.ApiService.Data.Entities;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.TestUrl;

public class Request
{
    public string Url { get; set; } = string.Empty;
    public Data.Entities.HttpMethod HttpMethod { get; set; } = Data.Entities.HttpMethod.GET;
    public string? Headers { get; set; }  // JSON 格式: {"Key": "Value"}
    public string? Body { get; set; }     // JSON 字串
}
