using FastEndpoints;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Products.Delete;

public class Request
{
    public int Id { get; set; }
}

public class Endpoint : Endpoint<Request>
{
    public override void Configure()
    {
        Delete("products/{Id}");
        AllowAnonymous();
        Description(x => x
            .WithName("DeleteProduct")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var existing = Data.Products.FirstOrDefault(p => p.Id == req.Id);

        if (existing is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        Data.Products.Remove(existing);
        await SendNoContentAsync(ct);
    }
}
