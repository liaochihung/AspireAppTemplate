using AspireAppTemplate.Web;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AspireAppTemplate.Web.Components.Pages
{
    public partial class CreateCustomJobDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

        private MudForm? _form;
        private bool _isValid;
        private DateTime? _scheduledDate = DateTime.Today.AddDays(1);
        private TimeSpan? _scheduledTime = new TimeSpan(9, 0, 0);

        public CustomJobsApiClient.CreateJobRequest Model { get; set; } = new() 
        { 
            Type = 1, 
            HttpMethod = 1 
        };

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
