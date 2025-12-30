using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace AspireAppTemplate.Web;

public class StorageApiClient(HttpClient httpClient)
{
    public async Task<string> UploadFileAsync(IBrowserFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
#pragma warning disable S5693 // 10 MB limit is intentional for file uploads
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10 MB limit
#pragma warning restore S5693
        using var streamContent = new StreamContent(stream);
        
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        
        content.Add(streamContent, "File", file.Name);

        var response = await httpClient.PostAsync("/api/v1/storage/upload", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken: ct);
        return result?.Url ?? string.Empty;
    }
}

public class UploadResponse
{
    public string Url { get; set; } = string.Empty;
}
