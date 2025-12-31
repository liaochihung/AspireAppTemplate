using AspireAppTemplate.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.Extensions.Localization;
using AspireAppTemplate.Shared.Resources;
using AspireAppTemplate.Shared;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class CustomJobs
    {
        private IEnumerable<CustomJobsApiClient.JobDto> _jobs = new List<CustomJobsApiClient.JobDto>();
        private bool _loading = true;
        private string _searchString = string.Empty;

        private Func<CustomJobsApiClient.JobDto, bool> _quickFilter => job =>
        {
            if (string.IsNullOrWhiteSpace(_searchString))
                return true;

            if (job.Name?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (job.Description?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (job.Url?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return false;
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadJobs();
        }

        private async Task LoadJobs()
        {
            _loading = true;
            try
            {
                await ExecuteWithAuthHandlingAsync(async () =>
                {
                    var response = await ApiClient.GetAllAsync();
                    _jobs = response.Jobs;
                });
            }
            catch (Exception ex)
            {
                Snackbar.Add(Loc["Job_LoadFail", ex.Message], Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task CreateJob()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<CreateCustomJobDialog>(Loc["Job_Create"], options);
            var result = await dialog.Result;
            
            if (result is not null && !result.Canceled && result.Data is CustomJobsApiClient.CreateJobRequest request)
            {
                try
                {
                    await ExecuteWithAuthHandlingAsync(async () =>
                    {
                        await ApiClient.CreateAsync(request);
                    });
                    Snackbar.Add(Loc["Job_Created"], Severity.Success);
                    await LoadJobs();
                }
                catch (Exception ex)
                {
                    Snackbar.Add(Loc["Job_CreateFail", ex.Message], Severity.Error);
                }
            }
        }

        private async Task ToggleJob(CustomJobsApiClient.JobDto job)
        {
            try
            {
                await ExecuteWithAuthHandlingAsync(async () =>
                {
                    await ApiClient.ToggleAsync(job.Id);
                });
                Snackbar.Add(Loc["Job_Toggled", job.IsActive ? Loc["Job_Disable"] : Loc["Job_Enable"]], Severity.Success);
                await LoadJobs();
            }
            catch (Exception ex)
            {
                Snackbar.Add(Loc["Job_ToggleFail", ex.Message], Severity.Error);
            }
        }

        private async Task DeleteJob(CustomJobsApiClient.JobDto job)
        {
            var result = await DialogService.ShowMessageBox(
               Loc["ConfirmDelete_Title"],
               Loc["Job_ConfirmDelete", job.Name],
               yesText: Loc["Delete"], cancelText: Loc["Cancel"]);

            if (result == true)
            {
                try
                {
                    await ExecuteWithAuthHandlingAsync(async () =>
                    {
                        await ApiClient.DeleteAsync(job.Id);
                    });
                    Snackbar.Add(Loc["Job_Deleted"], Severity.Success);
                    await LoadJobs();
                }
                catch (Exception ex)
                {
                    Snackbar.Add(Loc["Job_DeleteFail", ex.Message], Severity.Error);
                }
            }
        }

        private static Color GetHttpMethodColor(int method) => method switch
        {
            1 => Color.Info,    // GET
            2 => Color.Success, // POST
            3 => Color.Warning, // PUT
            4 => Color.Error,   // DELETE
            _ => Color.Default
        };

        private static string GetHttpMethodName(int method) => method switch
        {
            1 => "GET",
            2 => "POST",
            3 => "PUT",
            4 => "DELETE",
            _ => "UNKNOWN"
        };
    }
}
