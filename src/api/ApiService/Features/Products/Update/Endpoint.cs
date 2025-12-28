using FastEndpoints;
using FluentValidation;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Products.Update;

public class UpdateProductRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

public class UpdateProductValidator : Validator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}

public class Endpoint(AppDbContext dbContext, IOutputCacheStore cacheStore) : Endpoint<UpdateProductRequest, Product>
{
    public override void Configure()
    {
        Put("/products/{id}");
        Policies(AppPolicies.CanManageProducts);
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var id = Route<int>("id");
        var product = await dbContext.Products.FindAsync([id], cancellationToken: ct);

        if (product is null)
        {
            await this.SendResultAsync<Product>(Error.NotFound("Product.NotFound", "The product was not found."), ct: ct);
            return;
        }

        product.Name = req.Name;
        product.Price = req.Price;
        product.Description = req.Description;

        await dbContext.SaveChangesAsync(ct);
        await cacheStore.EvictByTagAsync("products", ct);

        ErrorOr<Product> result = product!;
        await this.SendResultAsync(result, ct: ct);
    }
}
