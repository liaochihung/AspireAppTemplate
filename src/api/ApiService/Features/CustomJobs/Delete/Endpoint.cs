using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Data.Entities;
using AspireAppTemplate.ApiService.Infrastructure.Jobs;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Hangfire;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.Delete;

public class Endpoint : Endpoint<Request>
{
    private readonly AppDbContext _db;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public Endpoint(AppDbContext db, IBackgroundJobClient backgroundJobClient)
    {
        _db = db;
        _backgroundJobClient = backgroundJobClient;
    }

    public override void Configure()
    {
        Delete("/custom-jobs/{id}");
        Policies(AppPolicies.CanManageSystem);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var job = await _db.CustomJobs.FindAsync([req.Id], ct);
        if (job == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        // 從 Hangfire 移除任務
        if (!string.IsNullOrEmpty(job.HangfireJobId))
        {
            if (job.Type == JobType.OneTime)
            {
                _backgroundJobClient.Delete(job.HangfireJobId);
            }
            else if (job.Type == JobType.Recurring)
            {
                RecurringJob.RemoveIfExists(job.HangfireJobId);
            }
        }

        _db.CustomJobs.Remove(job);
        await _db.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}
