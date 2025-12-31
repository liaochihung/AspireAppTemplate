using Microsoft.AspNetCore.Components;
using System.Net;

namespace AspireAppTemplate.Web.Infrastructure.Extensions;

/// <summary>
/// Extension methods for HttpClient to handle 401 Unauthorized responses
/// by automatically redirecting to logout.
/// </summary>
public static class HttpClientAuthExtensions
{
    /// <summary>
    /// Send GET request with automatic 401 handling.
    /// </summary>
    public static async Task<HttpResponseMessage> GetWithAuthCheckAsync(
        this HttpClient client,
        string requestUri,
        NavigationManager? navigationManager = null)
    {
        var response = await client.GetAsync(requestUri);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized && navigationManager != null)
        {
            navigationManager.NavigateTo("authentication/logout", forceLoad: true);
        }
        
        return response;
    }

    /// <summary>
    /// Get JSON with automatic 401 handling and redirection.
    /// </summary>
    public static async Task<T?> GetFromJsonWithAuthCheckAsync<T>(
        this HttpClient client,
        string requestUri,
        NavigationManager navigationManager)
    {
        var response = await client.GetAsync(requestUri);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            navigationManager.NavigateTo("authentication/logout", forceLoad: true);
            return default;
        }
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    /// <summary>
    /// POST JSON with automatic 401 handling.
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsJsonWithAuthCheckAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        NavigationManager? navigationManager = null)
    {
        var response = await client.PostAsJsonAsync(requestUri, value);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized && navigationManager != null)
        {
            navigationManager.NavigateTo("authentication/logout", forceLoad: true);
        }
        
        return response;
    }

    /// <summary>
    /// PUT JSON with automatic 401 handling.
    /// </summary>
    public static async Task<HttpResponseMessage> PutAsJsonWithAuthCheckAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        NavigationManager? navigationManager = null)
    {
        var response = await client.PutAsJsonAsync(requestUri, value);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized && navigationManager != null)
        {
            navigationManager.NavigateTo("authentication/logout", forceLoad: true);
        }
        
        return response;
    }

    /// <summary>
    /// DELETE with automatic 401 handling.
    /// </summary>
    public static async Task<HttpResponseMessage> DeleteWithAuthCheckAsync(
        this HttpClient client,
        string requestUri,
        NavigationManager? navigationManager = null)
    {
        var response = await client.DeleteAsync(requestUri);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized && navigationManager != null)
        {
            navigationManager.NavigateTo("authentication/logout", forceLoad: true);
        }
        
        return response;
    }
}
