namespace Construction.Core.DTOs;

/// <summary>
/// Standard query parameters for pagination, sorting, and filtering
/// </summary>
public class QueryParametersDto
{
    private int _page = 1;
    private int _pageSize = 50;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 50 : (value > 200 ? 200 : value);
    }

    public string? Sort { get; set; } // Format: "field:asc" or "field:desc"
    public string? Filter { get; set; } // Format: "key=value" (simple key-value pairs)
}
