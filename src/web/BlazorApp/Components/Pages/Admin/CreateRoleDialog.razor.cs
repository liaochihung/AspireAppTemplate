using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages.Admin
{
    public partial class CreateRoleDialog
    {
        [CascadingParameter] MudBlazor.IMudDialogInstance MudDialog { get; set; } = default!;

        private KeycloakRole Model { get; set; } = new KeycloakRole();
        private MudForm _form = default!;
        private bool _isValid;

        private void Cancel() => MudDialog.Cancel();

        private async Task Submit()
        {
            await _form.Validate();
            if (_form.IsValid)
            {
                MudDialog.Close(DialogResult.Ok(Model));
            }
        }
    }
}
