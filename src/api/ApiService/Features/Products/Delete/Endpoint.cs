using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Products.Delete;

public class Endpoint(AppDbContext dbContext, IOutputCacheStore cacheStore) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/products/{id}");
        Policies(AppPolicies.CanManageProducts);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var product = await dbContext.Products.FindAsync([id], cancellationToken: ct);

        if (product is null)
        {
            await this.SendResultAsync<Deleted>(Error.NotFound("Product.NotFound", "The product was not found."), ct: ct);
            return;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(ct);
        await cacheStore.EvictByTagAsync("products", ct);

        ErrorOr<Deleted> result = Result.Deleted;
        await this.SendResultAsync(result, ct: ct);
    }
}
