using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;

using AspireAppTemplate.ApiService.Services;

namespace AspireAppTemplate.ApiService.Features.Products.GetById;

public class Endpoint(AppDbContext dbContext, ICacheService cacheService) : EndpointWithoutRequest<Product>
{
    public override void Configure()
    {
        Get("/products/{id}");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var cacheKey = $"products:{id}";

        var product = await cacheService.GetOrSetAsync(cacheKey, async cancellationToken => 
        {
            return await dbContext.Products.FindAsync([id], cancellationToken: cancellationToken);
        }, TimeSpan.FromMinutes(10), ct);

        if (product is null)
        {
             await this.SendResultAsync<Product>(Error.NotFound("Product.NotFound", "The product was not found."), ct: ct);
             return;
        }

        ErrorOr<Product> result = product!;
        await this.SendResultAsync(result, ct: ct);
    }
}
