using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using AspireAppTemplate.ApiService.Services;

namespace AspireAppTemplate.ApiService.Features.Products.Delete;

public class Endpoint(AppDbContext dbContext, ICacheService cacheService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/products/{id}");
        Policies(AppPolicies.CanManageProducts);
        Version(1);
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
        
        await cacheService.RemoveAsync($"products:{id}", ct);

        ErrorOr<Deleted> result = Result.Deleted;
        await this.SendResultAsync(result, ct: ct);
    }
}
