using AspireAppTemplate.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages.Admin
{
    public partial class UserRolesDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public KeycloakUser User { get; set; } = default!;

        private List<KeycloakRole> _assignedRoles = new();
        private List<KeycloakRole> _availableRoles = new();
        private string _selectedRole = string.Empty;
        private bool _loading = true;

        protected override async Task OnInitializedAsync()
        {
            await LoadRoles();
        }

        private async Task LoadRoles()
        {
            _loading = true;
            try
            {
                if (string.IsNullOrEmpty(User.Id))
                {
                    Snackbar.Add("User ID is missing", Severity.Error);
                    return;
                }

                var assignedTask = IdentityClient.GetUserRolesAsync(User.Id);
                var availableTask = IdentityClient.GetRolesAsync();

                await Task.WhenAll(assignedTask, availableTask);

                _assignedRoles = (await assignedTask).ToList();
                _availableRoles = (await availableTask).Where(r => _assignedRoles.All(ar => ar.Name != r.Name)).ToList();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error loading roles: {ex.Message}", Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task AddRole()
        {
            if (string.IsNullOrEmpty(_selectedRole)) return;
            if (string.IsNullOrEmpty(User.Id))
            {
                Snackbar.Add("User ID is missing", Severity.Error);
                return;
            }

            try
            {
                await IdentityClient.AssignRoleAsync(User.Id, _selectedRole);
                Snackbar.Add($"Role {_selectedRole} assigned", Severity.Success);
                _selectedRole = string.Empty;
                await LoadRoles();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error assigning role: {ex.Message}", Severity.Error);
            }
        }

        private async Task RemoveRoleByName(string roleName)
        {
            if (string.IsNullOrEmpty(User.Id))
            {
                Snackbar.Add("User ID is missing", Severity.Error);
                return;
            }

            try
            {
                await IdentityClient.RemoveRoleAsync(User.Id, roleName);
                Snackbar.Add($"Role {roleName} removed", Severity.Success);
                await LoadRoles();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error removing role: {ex.Message}", Severity.Error);
            }
        }



        private void Close() => MudDialog.Close();
    }
}
