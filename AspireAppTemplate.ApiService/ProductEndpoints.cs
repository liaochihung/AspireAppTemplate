using Microsoft.AspNetCore.Authorization;


namespace AspireAppTemplate.ApiService;

public static class ProductEndpoints
{
    // In-memory store for demo purposes
    private static readonly List<Product> _products = new()
{
    new Product { Id = 1, Name = "Sample Product 1", Price = 9.99m, Description = "Demo product 1" },
    new Product { Id = 2, Name = "Sample Product 2", Price = 19.99m, Description = "Demo product 2" }
};

    private static int _nextId = 3;
    private static readonly object _lock = new();

    public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Product").WithTags(nameof(Product));

        group.MapGet("/", () => Results.Ok(_products))
            .WithName("GetAllProducts")
            .RequireAuthorization();

        group.MapGet("/{id}", (int id) =>
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        })
            .WithName("GetProductById")
            .RequireAuthorization();

        group.MapPut("/{id}", (int id, Product input) =>
        {
            if (input is null) return Results.BadRequest();

            lock (_lock)
            {
                var existing = _products.FirstOrDefault(p => p.Id == id);
                if (existing is null) return Results.NotFound();

                existing.Name = input.Name;
                existing.Price = input.Price;
                existing.Description = input.Description;

                return Results.NoContent();
            }
        })
            .WithName("UpdateProduct")
            .RequireAuthorization();
            //.RequireAuthorization(new AuthorizeAttribute { Roles = "Administrator" });

        group.MapPost("/", (Product model) =>
        {
            if (model is null) return Results.BadRequest();

            lock (_lock)
            {
                model.Id = _nextId++;
                _products.Add(model);
            }

            return Results.Created($"/api/Product/{model.Id}", model);
        })
            .WithName("CreateProduct")
            .RequireAuthorization();

        group.MapDelete("/{id}", (int id) =>
        {
            lock (_lock)
            {
                var existing = _products.FirstOrDefault(p => p.Id == id);
                if (existing is null) return Results.NotFound();

                _products.Remove(existing);
                return Results.Ok(existing);
            }
        })
            .WithName("DeleteProduct")
            .RequireAuthorization();
    }
}
