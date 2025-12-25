using FastEndpoints;
using FluentValidation;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Products.Update;

public class Request
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

public class Validator : Validator<Request>
{
    public Validator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("名稱不能為空");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("價格必須大於 0");
    }
}

public class Endpoint : Endpoint<Request>
{
    public override void Configure()
    {
        Put("products/{Id}");
        Roles("Administrator");
        Description(x => x
            .WithName("UpdateProduct")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        Logger.LogInformation("Updating product with ID: {Id}", req.Id);

        var existing = Data.Products.FirstOrDefault(p => p.Id == req.Id);

        if (existing is null)
        {
            Logger.LogWarning("Product with ID: {Id} not found for update", req.Id);
            await SendNotFoundAsync(ct);
            return;
        }

        existing.Name = req.Name;
        existing.Price = req.Price;
        existing.Description = req.Description;
        
        Logger.LogInformation("Product updated: {Id}", req.Id);

        await SendNoContentAsync(ct);
    }
}
