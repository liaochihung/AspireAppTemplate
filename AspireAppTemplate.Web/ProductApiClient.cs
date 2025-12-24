namespace AspireAppTemplate.Web;

public class ProductApiClient(HttpClient httpClient)
    {
        public async Task<Product[]> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                List<Product>? products = null;
                await foreach (var product in httpClient.GetFromJsonAsAsyncEnumerable<Product>("/api/Product", cancellationToken))
                {
                    products ??= [];
                    products.Add(product);
                }
                return products?.ToArray() ?? [];
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to get products: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error in GetProductsAsync: {ex.Message}", ex);
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await httpClient.GetFromJsonAsync<Product>($"/api/Product/{id}", cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to get product by id {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error in GetProductByIdAsync: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateProductAsync(int id, Product product, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"/api/Product/{id}", product, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update product {id}: {response.StatusCode} - {content}");
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to update product {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error in UpdateProductAsync: {ex.Message}", ex);
            }
        }

        public async Task<Product?> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("/api/Product", product, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Product>(cancellationToken: cancellationToken);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to create product: {response.StatusCode} - {content}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to create product: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error in CreateProductAsync: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await httpClient.DeleteAsync($"/api/Product/{id}", cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to delete product {id}: {response.StatusCode} - {content}");
                }
                return true;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to delete product {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error in DeleteProductAsync: {ex.Message}", ex);
            }
        }
    }
