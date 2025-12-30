using AspireAppTemplate.ApiService.Infrastructure.Storage;
using FastEndpoints;

namespace AspireAppTemplate.ApiService.Features.Storage.Delete;

public class DeleteRequest
{
    public string Path { get; set; } = default!;
}

public class Endpoint(IStorageService storageService) : Endpoint<DeleteRequest>
{
    public override void Configure()
    {
        Delete("/storage/files/{*Path}");
        AllowAnonymous(); // For demo
        Summary(s => 
        {
            s.Summary = "Delete a file";
            s.Description = "Deletes a file from storage.";
        });
    }

    public override async Task HandleAsync(DeleteRequest req, CancellationToken ct)
    {
        await storageService.RemoveAsync(req.Path, ct);
        await SendNoContentAsync(ct);
    }
}
