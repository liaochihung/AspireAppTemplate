using FastEndpoints;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Products.GetAll;

public class Endpoint : EndpointWithoutRequest<List<Product>>
{
    public override void Configure()
    {
        Get("products");
        AllowAnonymous(); // 先維持原本的邏輯，稍後我們會加上更細的權限控管範例
        Description(x => x
            .WithName("GetAllProducts")
            .WithTags("Products"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Logger.LogInformation("Retrieving all products. Count: {Count}", Data.Products.Count);
        await SendAsync(Data.Products, cancellation: ct);
    }
}
