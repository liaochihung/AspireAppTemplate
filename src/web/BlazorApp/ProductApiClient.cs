using System.Net.Http.Json;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.Web;

public class ProductApiClient(HttpClient httpClient)
{
    public async Task<Product[]> GetProductsAsync(CancellationToken ct = default)
    {
        var products = await httpClient.GetFromJsonAsync<Product[]>("/api/products", ct);
        return products ?? [];
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<Product>($"/api/products/{id}", ct);
    }

    public async Task<Product?> CreateProductAsync(Product product, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/products", product, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: ct);
    }

    public async Task UpdateProductAsync(int id, Product product, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/api/products/{id}", product, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/products/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}