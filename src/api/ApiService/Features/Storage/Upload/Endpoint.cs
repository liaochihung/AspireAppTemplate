using AspireAppTemplate.ApiService.Infrastructure.Storage;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.Storage.Upload;

public class UploadRequest
{
    public IFormFile File { get; set; } = default!;
}

public class UploadResponse
{
    public string Url { get; set; } = string.Empty;
}

public class Endpoint(IStorageService storageService) : Endpoint<UploadRequest, UploadResponse>
{
    public override void Configure()
    {
        Post("/storage/upload");
        AllowAnonymous(); // Enable for easy testing, enable Auth in production
        Version(1);
        AllowFileUploads();
        Summary(s => 
        {
            s.Summary = "Upload a file to MinIO storage";
            s.Description = "Uploads a file (image) and returns the public access URL.";
        });
    }

    public override async Task HandleAsync(UploadRequest req, CancellationToken ct)
    {
        var fileType = FileType.Image;
        var uri = await storageService.UploadAsync<Endpoint>(req.File, fileType, ct);
        
        // Return URL pointing to our own API Proxy (Get Endpoint)
        // This solves Mixed Content (HTTPS vs HTTP) and CORS issues.
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/v1/storage/files";
        
        // uri is relative path "folder/filename.ext"
        var fullUrl = $"{baseUrl}/{uri}";

        await SendOkAsync(new UploadResponse { Url = fullUrl }, ct);
    }
}
