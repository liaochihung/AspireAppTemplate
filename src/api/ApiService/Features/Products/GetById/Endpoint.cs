using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;

namespace AspireAppTemplate.ApiService.Features.Products.GetById;

public class Endpoint(AppDbContext dbContext) : EndpointWithoutRequest<Product>
{
    public override void Configure()
    {
        Get("/products/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<int>("id");
        var product = await dbContext.Products.FindAsync([id], cancellationToken: ct);

        if (product is null)
        {
             await this.SendResultAsync<Product>(Error.NotFound("Product.NotFound", "The product was not found."), ct: ct);
             return;
        }

        ErrorOr<Product> result = product!;
        await this.SendResultAsync(result, ct: ct);
    }
}
