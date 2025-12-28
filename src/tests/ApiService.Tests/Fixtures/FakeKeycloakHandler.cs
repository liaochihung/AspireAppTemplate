using System.Net;
using System.Text;
using System.Text.Json;

namespace AspireAppTemplate.ApiService.Tests.Fixtures;

/// <summary>
/// Mock HttpMessageHandler for Keycloak Admin API
/// </summary>
public class FakeKeycloakHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _routes = new();
    private readonly List<HttpRequestMessage> _receivedRequests = new();

    /// <summary>
    /// 設定 POST /admin/realms/{realm}/roles 的響應
    /// </summary>
    public void SetupCreateRole(HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes["POST:/admin/realms/test-realm/roles"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 DELETE /admin/realms/{realm}/roles/{name} 的響應
    /// </summary>
    public void SetupDeleteRole(string roleName, HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes[$"DELETE:/admin/realms/test-realm/roles/{roleName}"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 GET /admin/realms/{realm}/roles 的響應
    /// </summary>
    public void SetupGetAllRoles(HttpStatusCode statusCode, object? responseData = null)
    {
        _routes["GET:/admin/realms/test-realm/roles"] = _ => CreateJsonResponse(statusCode, responseData);
    }

    /// <summary>
    /// 設定 POST /admin/realms/{realm}/users 的響應
    /// </summary>
    public void SetupCreateUser(HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes["POST:/admin/realms/test-realm/users"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 PUT /admin/realms/{realm}/users/{id} 的響應
    /// </summary>
    public void SetupUpdateUser(string userId, HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes[$"PUT:/admin/realms/test-realm/users/{userId}"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 DELETE /admin/realms/{realm}/users/{id} 的響應
    /// </summary>
    public void SetupDeleteUser(string userId, HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes[$"DELETE:/admin/realms/test-realm/users/{userId}"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 GET /admin/realms/{realm}/users 的響應
    /// </summary>
    public void SetupGetAllUsers(HttpStatusCode statusCode, object? responseData = null)
    {
        _routes["GET:/admin/realms/test-realm/users"] = _ => CreateJsonResponse(statusCode, responseData);
    }

    /// <summary>
    /// 設定 POST /admin/realms/{realm}/users/{id}/role-mappings/realm 的響應
    /// </summary>
    public void SetupAssignRole(string userId, HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes[$"POST:/admin/realms/test-realm/users/{userId}/role-mappings/realm"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 DELETE /admin/realms/{realm}/users/{id}/role-mappings/realm 的響應
    /// </summary>
    public void SetupRemoveRole(string userId, HttpStatusCode statusCode, string? errorMessage = null)
    {
        _routes[$"DELETE:/admin/realms/test-realm/users/{userId}/role-mappings/realm"] = _ => CreateResponse(statusCode, errorMessage);
    }

    /// <summary>
    /// 設定 GET /admin/realms/{realm}/users/{id}/role-mappings/realm 的響應
    /// </summary>
    public void SetupGetUserRoles(string userId, HttpStatusCode statusCode, object? responseData = null)
    {
        _routes[$"GET:/admin/realms/test-realm/users/{userId}/role-mappings/realm"] = _ => CreateJsonResponse(statusCode, responseData);
    }

    /// <summary>
    /// 驗證是否收到特定請求
    /// </summary>
    public bool VerifyRequestSent(Func<HttpRequestMessage, bool> predicate)
    {
        return _receivedRequests.Any(predicate);
    }

    /// <summary>
    /// 取得所有收到的請求
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> ReceivedRequests => _receivedRequests.AsReadOnly();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _receivedRequests.Add(request);

        var key = $"{request.Method}:{request.RequestUri?.PathAndQuery}";
        
        if (_routes.TryGetValue(key, out var handler))
        {
            return Task.FromResult(handler(request));
        }

        // 預設回傳 404
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"Mock route not configured: {key}", Encoding.UTF8, "application/json")
        });
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? errorMessage = null)
    {
        var response = new HttpResponseMessage(statusCode);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            response.Content = new StringContent(errorMessage, Encoding.UTF8, "application/json");
        }
        return response;
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, object? data)
    {
        var response = new HttpResponseMessage(statusCode);
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        return response;
    }
}
