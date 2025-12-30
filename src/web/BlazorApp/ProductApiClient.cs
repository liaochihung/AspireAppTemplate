using System.Net.Http.Json;
using System.Web;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.Web;

public class ProductApiClient(HttpClient httpClient)
{
    public async Task<Product[]> GetProductsAsync(CancellationToken ct = default)
    {
        var result = await GetProductsPaginatedAsync(new PaginationRequest { Page = 1, PageSize = 1000 }, ct);
        return result.Items.ToArray();
    }

    public async Task<PaginatedResult<Product>> GetProductsPaginatedAsync(PaginationRequest request, CancellationToken ct = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["page"] = request.Page.ToString();
        query["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrEmpty(request.SearchTerm))
            query["searchTerm"] = request.SearchTerm;

        var result = await httpClient.GetFromJsonAsync<PaginatedResult<Product>>($"/api/v1/products?{query}", ct);
        return result ?? new PaginatedResult<Product>();
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        return await httpClient.GetFromJsonAsync<Product>($"/api/v1/products/{id}", ct);
    }

    public async Task<Product?> CreateProductAsync(Product product, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/products", product, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: ct);
    }

    public async Task UpdateProductAsync(int id, Product product, CancellationToken ct = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/api/v1/products/{id}", product, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProductAsync(int id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/products/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}