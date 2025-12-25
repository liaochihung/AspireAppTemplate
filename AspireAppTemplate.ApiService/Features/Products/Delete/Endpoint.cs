using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.Database;

namespace AspireAppTemplate.ApiService.Features.Products.Delete;

public class Request
{
    public int Id { get; set; }
}

public class Endpoint : Endpoint<Request>
{
    private readonly AppDbContext _db;

    public Endpoint(AppDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("products/{Id}");
        Policies(AppPolicies.CanManageProducts);
        Description(x => x
            .WithName("DeleteProduct")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        Logger.LogInformation("Deleting product with ID: {Id}", req.Id);

        var existing = await _db.Products.FindAsync([req.Id], ct);

        if (existing is null)
        {
            Logger.LogWarning("Product with ID: {Id} not found for deletion", req.Id);
            await SendNotFoundAsync(ct);
            return;
        }

        _db.Products.Remove(existing);
        await _db.SaveChangesAsync(ct);
        
        Logger.LogInformation("Product deleted: {Id}", req.Id);

        await SendNoContentAsync(ct);
    }
}
