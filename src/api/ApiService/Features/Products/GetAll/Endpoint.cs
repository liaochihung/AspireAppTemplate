using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Features.Products.GetAll;

public class Endpoint(AppDbContext dbContext) : EndpointWithoutRequest<IEnumerable<Product>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await dbContext.Products.ToListAsync(ct);
        ErrorOr<IEnumerable<Product>> result = products;
        await this.SendResultAsync(result, ct: ct);
    }
}
