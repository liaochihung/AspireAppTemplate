using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;

namespace AspireAppTemplate.ApiService.Features.Products.GetById;

public class Request
{
    public int Id { get; set; }
}

public class Endpoint : Endpoint<Request, Product>
{
    private readonly AppDbContext _db;

    public Endpoint(AppDbContext db) => _db = db;

    public override void Configure()
    {
        Get("products/{Id}");
        AllowAnonymous();
        Description(x => x
            .WithName("GetProductById")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        Logger.LogInformation("Retrieving product with ID: {Id}", req.Id);
        
        var product = await _db.Products.FindAsync([req.Id], ct);

        if (product is null)
        {
            Logger.LogWarning("Product with ID: {Id} not found", req.Id);
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(product, cancellation: ct);
    }
}
