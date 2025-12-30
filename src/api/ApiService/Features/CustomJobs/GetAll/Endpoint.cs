using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Features.CustomJobs.GetAll;

public class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly AppDbContext _db;

    public Endpoint(AppDbContext db)
    {
        _db = db;
    }

    public override void Configure()
    {
        Get("/custom-jobs");
        Policies(AppPolicies.CanManageSystem);
        Version(1);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var jobs = await _db.CustomJobs
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new JobDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Type = x.Type,
                CronExpression = x.CronExpression,
                ScheduledAt = x.ScheduledAt,
                HttpMethod = x.HttpMethod,
                Url = x.Url,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        await SendAsync(new Response { Jobs = jobs }, cancellation: ct);
    }
}
