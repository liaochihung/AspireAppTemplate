using FastEndpoints;
using AspireAppTemplate.Shared;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Infrastructure.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OutputCaching;

namespace AspireAppTemplate.ApiService.Features.Products.GetAll;

public class Endpoint(AppDbContext dbContext) : Endpoint<PaginationRequest, PaginatedResult<Product>>
{
    public override void Configure()
    {
        Get("/products");
        AllowAnonymous();
        Options(x => x.CacheOutput(c => c.Expire(TimeSpan.FromMinutes(5)).Tag("products")));
    }

    public override async Task HandleAsync(PaginationRequest req, CancellationToken ct)
    {
        var query = dbContext.Products.AsQueryable();

        // Apply search if provided
        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
        {
            var term = req.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(term) || 
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        var result = await query
            .OrderBy(p => p.Id)
            .ToPaginatedResultAsync(req, ct);

        await SendAsync(result, cancellation: ct);
    }
}
