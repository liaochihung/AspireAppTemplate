using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace AspireAppTemplate.Web;

public class StorageApiClient(HttpClient httpClient)
{
    public async Task<string> UploadFileAsync(IBrowserFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10 MB limit
        using var streamContent = new StreamContent(stream);
        
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        
        content.Add(streamContent, "File", file.Name);

        var response = await httpClient.PostAsync("/api/storage/upload", content, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UploadResponse>(cancellationToken: ct);
        return result?.Url ?? string.Empty;
    }

    public async Task<List<StorageFileDto>> GetFilesAsync(CancellationToken ct = default)
    {
        var response = await httpClient.GetFromJsonAsync<Response>("/api/storage/files", ct);
        return response?.Files ?? new List<StorageFileDto>();
    }

    public async Task DeleteFileAsync(string path, CancellationToken ct = default)
    {
        // Path needs to be part of the URL, handle slash if necessary. 
        // FastEndpoints wildcard mapping handles encoded slashes usually, but let's send path in URL.
        // If path contains actual slashes e.g. "folder/file.ext", we should ensure it's passed correctly.
        // Let's assume path is safe or we might need to url encode it if it causes issues.
        // In FastEndpoints wildcard: DELETE /storage/files/folder/file.ext works naturally.
        
        await httpClient.DeleteAsync($"/api/storage/files/{path}", ct);
    }

    public async Task<string> GetFileContentAsync(string path, CancellationToken ct = default)
    {
        // Reuse the proxy endpoint to get content
        return await httpClient.GetStringAsync($"/api/storage/files/{path}", ct);
    }
}

public class UploadResponse
{
    public string Url { get; set; } = string.Empty;
}

public class Response 
{
    public List<StorageFileDto> Files { get; set; } = new();
}

public class StorageFileDto
{
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
    public long Size { get; set; }
}
