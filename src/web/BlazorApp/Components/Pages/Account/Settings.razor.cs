using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace AspireAppTemplate.Web.Components.Pages.Account
{
    public partial class Settings
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthStateTask { get; set; } = default!;
        private AuthenticationState? _authState;

        protected override async Task OnInitializedAsync()
        {
            _authState = await AuthStateTask;
        }
    }
}
