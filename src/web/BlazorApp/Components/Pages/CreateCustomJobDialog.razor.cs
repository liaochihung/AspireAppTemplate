using AspireAppTemplate.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class CreateCustomJobDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;
        
        [Inject] private CustomJobsApiClient ApiClient { get; set; } = default!;
        [Inject] private ISnackbar Snackbar { get; set; } = default!;

        private MudForm? _form;
        private bool _isValid;
        private bool _testing;
        private CustomJobsApiClient.TestUrlResponse? _testResult;
        private DateTime? _scheduledDate = DateTime.Today.AddDays(1);
        private TimeSpan? _scheduledTime = new TimeSpan(9, 0, 0);

        public CustomJobsApiClient.CreateJobRequest Model { get; set; } = new() 
        { 
            Type = 1, 
            HttpMethod = 1 
        };

        private async Task TestUrl()
        {
            if (string.IsNullOrWhiteSpace(Model.Url)) return;

            _testing = true;
            _testResult = null;

            try
            {
                var request = new CustomJobsApiClient.TestUrlRequest
                {
                    Url = Model.Url,
                    HttpMethod = Model.HttpMethod,
                    Headers = Model.Headers,
                    Body = Model.Body
                };

                _testResult = await ApiClient.TestUrlAsync(request);

                if (_testResult.IsSuccess)
                {
                    Snackbar.Add($"測試成功！HTTP {_testResult.StatusCode} ({_testResult.LatencyMs}ms)", Severity.Success);
                }
                else
                {
                    Snackbar.Add($"測試失敗: {_testResult.ErrorMessage ?? $"HTTP {_testResult.StatusCode}"}", Severity.Warning);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"測試失敗: {ex.Message}", Severity.Error);
            }
            finally
            {
                _testing = false;
            }
        }

        private void Cancel()
        {
            MudDialog?.Cancel();
        }

        private void Submit()
        {
            if (_form == null || !_isValid) return;

            // 組合 ScheduledAt
            if (Model.Type == 1 && _scheduledDate.HasValue && _scheduledTime.HasValue)
            {
                Model.ScheduledAt = _scheduledDate.Value.Add(_scheduledTime.Value).ToUniversalTime();
            }

            MudDialog?.Close(DialogResult.Ok(Model));
        }
    }
}
