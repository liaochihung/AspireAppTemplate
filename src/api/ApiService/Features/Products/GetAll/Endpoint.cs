using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Products.GetAll;

public class Endpoint(AppDbContext dbContext) : EndpointWithoutRequest<IEnumerable<Product>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
        Options(x => x.CacheOutput(c => c.Expire(TimeSpan.FromMinutes(5)).Tag("products")));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await dbContext.Products.ToListAsync(ct);
        ErrorOr<IEnumerable<Product>> result = products;
        await this.SendResultAsync(result, ct: ct);
    }
}
