using FastEndpoints;
using FluentValidation;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;

namespace AspireAppTemplate.ApiService.Features.Products.Create;

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

public class CreateProductValidator : Validator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}

public class Endpoint(AppDbContext dbContext) : Endpoint<CreateProductRequest, Product>
{
    public override void Configure()
    {
        Post("/products");
        Policies(AppPolicies.CanManageProducts);
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = new Product
        {
            Name = req.Name,
            Price = req.Price,
            Description = req.Description
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(ct);

        // We wrap the result in ErrorOr, explicitly casting or letting implicit conversion handle it if supported, 
        // but here we are sending the result using the extension method which expects ErrorOr<T>
        
        ErrorOr<Product> result = product;

        await this.SendResultAsync(result, ct: ct);
    }
}
