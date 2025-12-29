using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Data.Entities;
using AspireAppTemplate.Shared.Models;
using AspireAppTemplate.Shared;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Features.AuditLogs.GetList;

public class Endpoint : EndpointWithoutRequest<List<AuditLogDto>>
{
    private readonly AppDbContext _dbContext;

    public Endpoint(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/audit-logs");
        // We might want to restrict this to Admins only
        Policies(AppPolicies.CanManageSystem); 
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Simple implementation first, can add pagination/filtering later if needed for "Basic Function"
        // But for "Modern and Professional", pagination is usually expected.
        // Let's return the last 100 logs for now to keep it safe, 
        // or actually, let's just dump the top 50 descending.
        
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.Timestamp)
            .Take(100)
            .Select(x => new AuditLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.UserName,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Timestamp = x.Timestamp,
                OldValues = x.OldValues,
                NewValues = x.NewValues
            })
            .ToListAsync(ct);

        await SendOkAsync(logs, ct);
    }
}
