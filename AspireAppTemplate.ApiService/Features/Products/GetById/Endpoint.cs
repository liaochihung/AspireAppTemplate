using FastEndpoints;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Products.GetById;

public class Request
{
    public int Id { get; set; }
}

public class Endpoint : Endpoint<Request, Product>
{
    public override void Configure()
    {
        Get("products/{Id}");
        AllowAnonymous();
        Description(x => x
            .WithName("GetProductById")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var product = Data.Products.FirstOrDefault(p => p.Id == req.Id);

        if (product is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendAsync(product, cancellation: ct);
    }
}
