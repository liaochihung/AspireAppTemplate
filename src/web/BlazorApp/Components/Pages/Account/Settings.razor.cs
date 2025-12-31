using Microsoft.AspNetCore.Components;
using MudBlazor;
using AspireAppTemplate.Shared;
using Microsoft.Extensions.Options;

namespace AspireAppTemplate.Web.Components.Pages.Account
{
    public partial class Settings
    {
        [Inject] private IOptions<FeatureFlags> FeatureFlagsOptions { get; set; } = default!;

        private FeatureFlags _featureFlags = new();
        private KeycloakUser? _userModel;
        private bool _saving;
        private bool _loading = true;

        protected override async Task OnInitializedAsync()
        {
            _featureFlags = FeatureFlagsOptions.Value;
            await LoadUserProfile();
        }

        private async Task LoadUserProfile()
        {
            _loading = true;
            try 
            {
                // Fetch profile from API endpoint
                _userModel = await IdentityClient.GetMyProfileAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to load user profile");
                Snackbar.Add(Loc["Settings_LoadFail", ex.Message], Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task SaveChanges()
        {
            _saving = true;
            try
            {
                if (_userModel == null) return;
                
                await IdentityClient.UpdateMyProfileAsync(_userModel.FirstName, _userModel.LastName);
                Snackbar.Add(Loc["Settings_Saved"], Severity.Success);
                
                // Reload to confirm changes
                await LoadUserProfile();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save profile");
                Snackbar.Add(Loc["Settings_SaveFail", ex.Message], Severity.Error);
            }
            finally
            {
                _saving = false;
            }
        }

        private async Task TriggerAction(string action)
        {
            var userId = _userModel?.Id.ToString();
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                await IdentityClient.ExecuteActionsEmailAsync(userId, new List<string> { action });
                Snackbar.Add(Loc["Settings_EmailSent"], Severity.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to trigger action {Action}", action);
                Snackbar.Add(Loc["Settings_ActionFail", ex.Message], Severity.Error);
            }
        }
    }
}
