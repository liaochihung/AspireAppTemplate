using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Features.Products.GetAll;

public class Endpoint : EndpointWithoutRequest<List<Product>>
{
    private readonly AppDbContext _db;

    public Endpoint(AppDbContext db) => _db = db;

    public override void Configure()
    {
        Get("products");
        AllowAnonymous();
        Description(x => x
            .WithName("GetAllProducts")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await _db.Products.ToListAsync(ct);
        Logger.LogInformation("Retrieving all products. Count: {Count}", products.Count);
        await SendAsync(products, cancellation: ct);
    }
}
