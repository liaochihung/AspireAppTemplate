using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class Token
    {
        private string currentToken = string.Empty;
        private bool isLoading = true;
        private string? errorMessage;

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            errorMessage = null;
            try
            {
                var httpContext = httpContextAccessor.HttpContext ??
                    throw new InvalidOperationException("No HttpContext available from the IHttpContextAccessor.");

                var accessToken = await httpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    currentToken = accessToken;
                }
                else
                {
                    errorMessage = "No access token found.";
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task CopyToken()
        {
            if (!string.IsNullOrEmpty(currentToken))
            {
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", currentToken);
                Snackbar.Add("Token copied to clipboard", Severity.Success);
            }
        }
    }
}
