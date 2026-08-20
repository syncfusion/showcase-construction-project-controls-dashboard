namespace Construction.Core.DTOs;

/// <summary>
/// Standard paged response wrapper for API endpoints
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class PagedResponseDto<T>
{
    public IEnumerable<T> Data { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
