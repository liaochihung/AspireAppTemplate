using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace AspireAppTemplate.Web;

/// <summary>
/// HTTP Message Handler that attaches access tokens to outgoing requests.
/// </summary>
public class AuthorizationHandler(IHttpContextAccessor httpContextAccessor, ILogger<AuthorizationHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            logger.LogWarning("HttpContext is null - cannot attach token");
            return await base.SendAsync(request, cancellationToken);
        }

        try
        {
            var accessToken = await httpContext.GetTokenAsync("access_token");
            
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                logger.LogWarning("No access_token found in HttpContext for request to {Uri}", request.RequestUri);
            }
            else
            {
                logger.LogInformation("Attaching Bearer token to request: {Uri}", request.RequestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting access token from HttpContext");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}