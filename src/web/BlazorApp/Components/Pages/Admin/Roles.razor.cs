using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.Extensions.Localization;
using AspireAppTemplate.Shared.Resources;

namespace AspireAppTemplate.Web.Components.Pages.Admin
{
    public partial class Roles
    {


        private IEnumerable<KeycloakRole> _roles = new List<KeycloakRole>();
        private bool _loading = true;
        private string _searchString = string.Empty;

        private Func<KeycloakRole, bool> _quickFilter => role =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (role.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (role.Description?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadRoles();
        }

        private async Task LoadRoles()
        {
            _loading = true;
            try
            {
                await ExecuteWithAuthHandlingAsync(async () =>
                {
                    _roles = await IdentityClient.GetRolesAsync();
                });
            }
            catch (Exception ex)
            {
                Snackbar.Add(Loc["Role_LoadFail", ex.Message], Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task CreateRole()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<CreateRoleDialog>(Loc["Role_Create"], options);
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is KeycloakRole role)
            {
                try
                {
                    await ExecuteWithAuthHandlingAsync(async () =>
                    {
                        await IdentityClient.CreateRoleAsync(role);
                    });
                    Snackbar.Add(Loc["Role_Created"], Severity.Success);
                    await LoadRoles();
                }
                catch (Exception ex)
                {
                    Snackbar.Add(Loc["Role_CreateFail", ex.Message], Severity.Error);
                }
            }
        }

        private async Task DeleteRole(KeycloakRole role)
        {
            var result = await DialogService.ShowMessageBox(
               Loc["ConfirmDelete_Title"],
               Loc["Role_ConfirmDelete", role.Name ?? ""],
               yesText: Loc["Delete"], cancelText: Loc["Cancel"]);

            if (result == true)
            {
                try
                {
                    await ExecuteWithAuthHandlingAsync(async () =>
                    {
                        await IdentityClient.DeleteRoleAsync(role.Name);
                    });
                    Snackbar.Add(Loc["Role_Deleted"], Severity.Success);
                    await LoadRoles();
                }
                catch (Exception ex)
                {
                    Snackbar.Add(Loc["Role_DeleteFail", ex.Message], Severity.Error);
                }
            }
        }
    }
}
