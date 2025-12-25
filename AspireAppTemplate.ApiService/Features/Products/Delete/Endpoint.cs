using FastEndpoints;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Products.Delete;

public class Request
{
    public int Id { get; set; }
}

public class Endpoint : Endpoint<Request>
{
    public override void Configure()
    {
        Delete("products/{Id}");
        Roles(AppRoles.Administrator);
        Description(x => x
            .WithName("DeleteProduct")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        Logger.LogInformation("Deleting product with ID: {Id}", req.Id);

        var existing = Data.Products.FirstOrDefault(p => p.Id == req.Id);

        if (existing is null)
        {
            Logger.LogWarning("Product with ID: {Id} not found for deletion", req.Id);
            await SendNotFoundAsync(ct);
            return;
        }

        Data.Products.Remove(existing);
        
        Logger.LogInformation("Product deleted: {Id}", req.Id);

        await SendNoContentAsync(ct);
    }
}
