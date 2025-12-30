using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;
using AspireAppTemplate.ApiService.Services;

namespace AspireAppTemplate.ApiService.Features.Products.GetAll;

public class Endpoint(AppDbContext dbContext, ICacheService cacheService) : Endpoint<PaginationRequest, PaginatedResult<Product>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(PaginationRequest req, CancellationToken ct)
    {
        var cacheKey = $"products:list:{req.Page}:{req.PageSize}:{req.SearchTerm}";

        // Cache for 1 minute (eventual consistency for lists)
        var result = await cacheService.GetOrSetAsync(cacheKey, async cancellationToken => 
        {
            var query = dbContext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(req.SearchTerm))
            {
                var term = req.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(term) || 
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            return await query
                .OrderBy(p => p.Id)
                .ToPaginatedResultAsync(req, cancellationToken);
        }, TimeSpan.FromMinutes(1), ct);

        await SendAsync(result!, cancellation: ct);
    }
}
