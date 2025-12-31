using AspireAppTemplate.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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
                Snackbar.Add($"任務載入失敗: {ex.Message}", Severity.Error);
            }
            finally
            {
                _loading = false;
            }
        }

        private async Task CreateJob()
        {
            var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<CreateCustomJobDialog>("新增自訂任務", options);
            var result = await dialog.Result;
            
            if (result is not null && !result.Canceled && result.Data is CustomJobsApiClient.CreateJobRequest request)
            {
                try
                {
                    await ExecuteWithAuthHandlingAsync(async () =>
                    {
                        await ApiClient.CreateAsync(request);
                    });
                    Snackbar.Add("任務已新增", Severity.Success);
                    await LoadJobs();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"任務新增失敗: {ex.Message}", Severity.Error);
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
                Snackbar.Add($"任務已{(job.IsActive ? "停用" : "啟用")}", Severity.Success);
                await LoadJobs();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"操作失敗: {ex.Message}", Severity.Error);
            }
        }

        private async Task DeleteJob(CustomJobsApiClient.JobDto job)
        {
            var result = await DialogService.ShowMessageBox(
               "刪除任務",
               $"確定要刪除 {job.Name} 嗎?",
               yesText: "刪除", cancelText: "取消");

            if (result == true)
            {
                try
                {
                    await ExecuteWithAuthHandlingAsync(async () =>
                    {
                        await ApiClient.DeleteAsync(job.Id);
                    });
                    Snackbar.Add("任務已刪除", Severity.Success);
                    await LoadJobs();
                }
                catch (Exception ex)
                {
                    Snackbar.Add($"任務刪除失敗: {ex.Message}", Severity.Error);
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
