using AspireAppTemplate.Shared;

namespace AspireAppTemplate.ApiService.Features.Products;

public static class Data
{
    public static readonly List<Product> Products = new()
    {
        new Product { Id = 1, Name = "Sample Product 1", Price = 9.99m, Description = "Demo product 1" },
        new Product { Id = 2, Name = "Sample Product 2", Price = 19.99m, Description = "Demo product 2" }
    };

    private static int _nextId = 3;
    private static readonly object _lock = new();

    public static int GetNextId()
    {
        lock (_lock)
        {
            return _nextId++;
        }
    }
}
