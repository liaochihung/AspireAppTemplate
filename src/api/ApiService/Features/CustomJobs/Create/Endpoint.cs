using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Data.Entities;
using AspireAppTemplate.ApiService.Infrastructure.Jobs;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Hangfire;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.Create;

public class Endpoint : Endpoint<Request, Response>
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
        Post("/custom-jobs");
        Policies(AppPolicies.CanManageSystem);
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var job = new CustomJob
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description,
            Type = req.Type,
            CronExpression = req.CronExpression,
            ScheduledAt = req.ScheduledAt,
            HttpMethod = req.HttpMethod,
            Url = req.Url,
            Headers = req.Headers,
            Body = req.Body,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = User.Identity?.Name ?? "System"
        };

        // 註冊到 Hangfire
        if (job.Type == JobType.OneTime)
        {
            if (job.ScheduledAt == null)
            {
                await SendErrorsAsync(cancellation: ct);
                return;
            }

            var hangfireJobId = _backgroundJobClient.Schedule<HttpJobExecutor>(
                x => x.ExecuteAsync(job.Id, null),
                job.ScheduledAt.Value);
            
            job.HangfireJobId = hangfireJobId;
        }
        else if (job.Type == JobType.Recurring)
        {
            if (string.IsNullOrEmpty(job.CronExpression))
            {
                await SendErrorsAsync(cancellation: ct);
                return;
            }

            var recurringJobId = $"custom-job-{job.Id}";
            RecurringJob.AddOrUpdate<HttpJobExecutor>(
                recurringJobId,
                x => x.ExecuteAsync(job.Id, null),
                job.CronExpression);
            
            job.HangfireJobId = recurringJobId;
        }

        _db.CustomJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        await SendAsync(new Response
        {
            Id = job.Id,
            Name = job.Name,
            Description = job.Description,
            Type = job.Type,
            CronExpression = job.CronExpression,
            ScheduledAt = job.ScheduledAt,
            HttpMethod = job.HttpMethod,
            Url = job.Url,
            IsActive = job.IsActive,
            CreatedAt = job.CreatedAt
        }, cancellation: ct);
    }
}
