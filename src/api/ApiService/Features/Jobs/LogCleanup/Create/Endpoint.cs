using FastEndpoints;
using Hangfire;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Infrastructure.Jobs;

namespace AspireAppTemplate.ApiService.Features.Jobs.LogCleanup.Create;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Post("/jobs/log-cleanup");
        Policies(AppPolicies.CanManageSystem);
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        var jobId = BackgroundJob.Schedule<LogCleanupJob>(
            job => job.ExecuteAsync(req.RetentionDays, ct),
            TimeSpan.FromDays(req.ScheduleDays)
        );

        await SendOkAsync(new Response(jobId), ct);
    }
}

public record Request(int RetentionDays, int ScheduleDays = 30);
public record Response(string JobId);
