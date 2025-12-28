using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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
                _users = await IdentityClient.GetUsersAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading users: {ex.Message}", Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task OpenUserDialog(KeycloakUser? user = null)
        {
            var title = user == null ? "新增使用者" : "編輯使用者";
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
                        Snackbar.Add("使用者已新增", Severity.Success);
                    }
                    else
                    {
                        await IdentityClient.UpdateUserAsync(savedModel);
                        Snackbar.Add("使用者已更新", Severity.Success);
                    }
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"使用者儲存失敗: {ex.Message}", Severity.Error);
                }
            }
        }

        private async Task DeleteUser(KeycloakUser user)
        {
            var result = await DialogService.ShowMessageBox(
                "Delete User",
                $"Are you sure you want to delete {user.Username}?",
                yesText: "Delete", cancelText: "Cancel");

            if (result == true)
            {
                try
                {
                    if (string.IsNullOrEmpty(user.Id))
                        throw new InvalidOperationException("使用者 ID 為空");

                    await IdentityClient.DeleteUserAsync(user.Id);
                    Snackbar.Add("使用者已刪除", Severity.Success);
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"使用者刪除失敗: {ex.Message}", Severity.Error);
                }
            }
        }

        private async Task ManageRoles(KeycloakUser user)
        {
            var parameters = new DialogParameters<UserRolesDialog> { { x => x.User, user } };
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            await DialogService.ShowAsync<UserRolesDialog>($"Roles for {user.Username}", parameters, options);
        }
    }
}
