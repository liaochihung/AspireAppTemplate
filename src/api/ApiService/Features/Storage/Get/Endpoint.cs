using AspireAppTemplate.ApiService.Infrastructure.Storage;
using FastEndpoints;
using Microsoft.AspNetCore.StaticFiles;

namespace AspireAppTemplate.ApiService.Features.Storage.Get;

public class GetRequest
{
    public string Path { get; set; } = default!;
}

public class Endpoint(IStorageService storageService, ILogger<Endpoint> logger) : Endpoint<GetRequest>
{
    public override void Configure()
    {
        Get("/storage/files/{*Path}"); // Capital P to match property
        AllowAnonymous();
        Version(1);
        Summary(s => 
        {
            s.Summary = "Download a file";
            s.Description = "Streams the file content from storage.";
        });
    }

    public override async Task HandleAsync(GetRequest req, CancellationToken ct)
    {
        logger.LogInformation("Downloading file: {Path}", req.Path);

        // Basic MIME type detection
        new FileExtensionContentTypeProvider().TryGetContentType(req.Path, out var contentType);
        contentType ??= "application/octet-stream";

        try 
        {
            var stream = await storageService.GetFileAsync(req.Path, ct);
            await SendStreamAsync(
                stream: stream,
                contentType: contentType,
                fileLengthBytes: stream.Length, 
                cancellation: ct
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading file: {Path}", req.Path);
            await SendNotFoundAsync(ct);
        }
    }
}
