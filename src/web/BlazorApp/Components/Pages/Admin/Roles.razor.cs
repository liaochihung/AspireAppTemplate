using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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
                _roles = await IdentityClient.GetRolesAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"角色載入失敗: {ex.Message}", Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task CreateRole()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
            var dialog = await DialogService.ShowAsync<CreateRoleDialog>("Create Role", options);
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is KeycloakRole role)
            {
                try
                {
                    await IdentityClient.CreateRoleAsync(role);
                    Snackbar.Add("角色已新增", Severity.Success);
                    await LoadRoles();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"角色新增失敗: {ex.Message}", Severity.Error);
                }
            }
        }

        private async Task DeleteRole(KeycloakRole role)
        {
            var result = await DialogService.ShowMessageBox(
               "刪除角色",
               $"確定要刪除 {role.Name} 嗎?",
               yesText: "刪除", cancelText: "取消");

            if (result == true)
            {
                try
                {
                    await IdentityClient.DeleteRoleAsync(role.Name);
                    Snackbar.Add("角色已刪除", Severity.Success);
                    await LoadRoles();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"角色刪除失敗: {ex.Message}", Severity.Error);
                }
            }
        }
    }
}
