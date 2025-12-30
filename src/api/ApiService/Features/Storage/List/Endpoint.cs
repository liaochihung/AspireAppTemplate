using AspireAppTemplate.ApiService.Infrastructure.Storage;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.Storage.List;

public class StorageFileDto
{
    public string Name { get; set; } = default!;
    public string Url { get; set; } = default!;
    public long Size { get; set; }
}

public class Response 
{
    public List<StorageFileDto> Files { get; set; } = new();
}

public class Endpoint(IStorageService storageService) : EndpointWithoutRequest<Response>
{
    public override void Configure()
    {
        Get("/storage/files");
        AllowAnonymous(); // For demo
        Summary(s => 
        {
            s.Summary = "List all files";
            s.Description = "List all files in storage.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var files = await storageService.ListFilesAsync(ct);
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/storage/files";

        var response = new Response
        {
            Files = files.Select(f => new StorageFileDto 
            {
                Name = f.Name,
                Size = f.Size,
                Url = $"{baseUrl}/{f.Name}"
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}
