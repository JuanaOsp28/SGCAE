using Microsoft.EntityFrameworkCore;

namespace SGCAE.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Normaliza página y tamaño para no recuperar grandes volúmenes de una sola vez.</summary>
    public static (int Page, int PageSize) Normalize(int? page, int? pageSize) =>
        (Math.Max(page ?? 1, 1), Math.Clamp(pageSize ?? DefaultPageSize, 1, MaxPageSize));

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var (p, size) = Normalize(page, pageSize);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((p - 1) * size).Take(size).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, p, size, total);
    }
}
