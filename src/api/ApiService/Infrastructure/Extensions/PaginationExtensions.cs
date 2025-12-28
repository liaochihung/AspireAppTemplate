using AspireAppTemplate.Shared;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Infrastructure.Extensions;

/// <summary>
/// IQueryable 分頁擴展方法
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// 將查詢轉換為分頁結果
    /// </summary>
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest request,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);
        
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<T>(items, totalCount, request.Page, request.PageSize);
    }

    /// <summary>
    /// 將集合轉換為分頁結果 (用於非 EF 資料來源如 Keycloak)
    /// </summary>
    public static PaginatedResult<T> ToPaginatedResult<T>(
        this IEnumerable<T> items,
        PaginationRequest request)
    {
        var list = items.ToList();
        var totalCount = list.Count;

        var paged = list
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResult<T>(paged, totalCount, request.Page, request.PageSize);
    }

    /// <summary>
    /// 套用搜尋過濾 (簡易字串比對)
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? searchTerm,
        Func<IQueryable<T>, string, IQueryable<T>> searchFunc)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        return searchFunc(query, searchTerm);
    }
}
