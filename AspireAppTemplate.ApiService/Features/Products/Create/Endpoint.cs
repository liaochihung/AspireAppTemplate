using FastEndpoints;
using FluentValidation;
using AspireAppTemplate.Shared;
using AspireAppTemplate.Database;

namespace AspireAppTemplate.ApiService.Features.Products.Create;

public class Request
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("產品名稱不能為空");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("價格必須大於 0");
    }
}

public class Endpoint : Endpoint<Request, Product>
{
    private readonly AppDbContext _db;

    public Endpoint(AppDbContext db) => _db = db;

    public override void Configure()
    {
        Post("products");
        Policies(AppPolicies.CanManageProducts); 
        Description(x => x
            .WithName("CreateProduct")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        Logger.LogInformation("Creating product: {Name}", req.Name);

        var product = new Product
        {
            Name = req.Name,
            Price = req.Price,
            Description = req.Description
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        
        Logger.LogInformation("Product created with ID: {Id}", product.Id);

        await SendCreatedAtAsync<GetAll.Endpoint>(new { }, product, cancellation: ct);
    }
}
