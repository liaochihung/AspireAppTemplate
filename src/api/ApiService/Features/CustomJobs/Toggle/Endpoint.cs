using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Data.Entities;
using AspireAppTemplate.ApiService.Infrastructure.Jobs;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Hangfire;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.Toggle;

public class Endpoint : Endpoint<Request, Response>
{
    private readonly AppDbContext _db;

    public Endpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Post("/custom-jobs/{id}/toggle");
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

        job.IsActive = !job.IsActive;
        job.UpdatedAt = DateTime.UtcNow;
        job.UpdatedBy = User.Identity?.Name ?? "System";

        if (job.Type == JobType.Recurring && !string.IsNullOrEmpty(job.HangfireJobId))
        {
            if (job.IsActive)
            {
                // 重新啟用
                RecurringJob.AddOrUpdate<HttpJobExecutor>(
                    job.HangfireJobId,
                    x => x.ExecuteAsync(job.Id, null),
                    job.CronExpression!);
            }
            else
            {
                // 停用
                RecurringJob.RemoveIfExists(job.HangfireJobId);
            }
        }

        await _db.SaveChangesAsync(ct);

        await SendAsync(new Response
        {
            Id = job.Id,
            IsActive = job.IsActive
        }, cancellation: ct);
    }
}
