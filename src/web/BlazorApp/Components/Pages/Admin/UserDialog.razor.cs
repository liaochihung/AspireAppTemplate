using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages.Admin
{
    public partial class UserDialog
    {
        [CascadingParameter] MudBlazor.IMudDialogInstance MudDialog { get; set; } = default!;

        [Parameter] public KeycloakUser Model { get; set; } = new KeycloakUser();

        private string Password { get; set; } = string.Empty;
        private MudForm _form = default!;
        private bool _isValid;

        private bool _passwordShow;
        private InputType _passwordInput = InputType.Password;
        private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

        private void TogglePasswordVisibility()
        {
            if (_passwordShow)
            {
                _passwordShow = false;
                _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
                _passwordInput = InputType.Password;
            }
            else
            {
                _passwordShow = true;
                _passwordInputIcon = Icons.Material.Filled.Visibility;
                _passwordInput = InputType.Text;
            }
        }

        private void Cancel() => MudDialog.Cancel();

        private async Task Submit()
        {
            await _form.Validate();
            if (_form.IsValid)
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    Model.Credentials = new List<KeycloakCredential>
                    {
                        new KeycloakCredential { Type = "password", Value = Password, Temporary = false }
                    };
                }

                MudDialog.Close(DialogResult.Ok(Model));
            }
        }
    }
}
