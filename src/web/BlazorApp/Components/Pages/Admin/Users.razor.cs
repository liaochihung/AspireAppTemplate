using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.Extensions.Localization;
using AspireAppTemplate.Shared.Resources;

namespace AspireAppTemplate.Web.Components.Pages.Admin
{
    public partial class Users
    {


        private IEnumerable<KeycloakUser> _users = new List<KeycloakUser>();
        private bool _loading = true;
        private string _searchString = string.Empty;

        private Func<KeycloakUser, bool> _quickFilter => user =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (user.Username?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (user.Email?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (user.FirstName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (user.LastName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            _loading = true;
            try
            {
                await ExecuteWithAuthHandlingAsync(async () =>
                {
                    _users = await IdentityClient.GetUsersAsync();
                });
            }
            catch (Exception ex)
            {
                Snackbar.Add(Loc["User_LoadFail", ex.Message], Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task OpenUserDialog(KeycloakUser? user = null)
        {
            var title = user == null ? Loc["User_Create"] : Loc["User_Edit"];
            // Clone if editing to avoid mutating grid item directly before save
            var model = user == null ? new KeycloakUser() : new KeycloakUser
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Enabled = user.Enabled
            };

            var parameters = new DialogParameters<UserDialog> { { x => x.Model, model } };

            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<UserDialog>(title, parameters, options);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled && result.Data is KeycloakUser savedModel)
            {
                try
                {
                    if (string.IsNullOrEmpty(savedModel.Id))
                    {
                        await IdentityClient.CreateUserAsync(savedModel);
                        Snackbar.Add(Loc["User_Created"], Severity.Success);
                    }
                    else
                    {
                        await IdentityClient.UpdateUserAsync(savedModel);
                        Snackbar.Add(Loc["User_Updated"], Severity.Success);
                    }
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    Snackbar.Add(Loc["User_SaveFail", ex.Message], Severity.Error);
                }
            }
        }

        private async Task DeleteUser(KeycloakUser user)
        {
            var result = await DialogService.ShowMessageBox(
                Loc["ConfirmDelete_Title"],
                Loc["ConfirmDelete_Content", user.Username],
                yesText: Loc["Delete"], cancelText: Loc["Cancel"]);

            if (result == true)
            {
                try
                {
                    if (string.IsNullOrEmpty(user.Id))
                        throw new InvalidOperationException("User ID is empty");

                    await IdentityClient.DeleteUserAsync(user.Id);
                    Snackbar.Add(Loc["User_Deleted"], Severity.Success);
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    Snackbar.Add(Loc["User_DeleteFail", ex.Message], Severity.Error);
                }
            }
        }

        private async Task ManageRoles(KeycloakUser user)
        {
            var parameters = new DialogParameters<UserRolesDialog> { { x => x.User, user } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            await DialogService.ShowAsync<UserRolesDialog>(Loc["User_AssignRoleTitle", user.Username ?? ""], parameters, options);
        }
    }
}
