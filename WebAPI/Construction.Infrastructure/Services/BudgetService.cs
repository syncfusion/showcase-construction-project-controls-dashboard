using Construction.Core.DTOs;
using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _repo;

    public BudgetService(IBudgetRepository repo) => _repo = repo;

    public async Task<PagedResponseDto<BudgetDto>> GetBudgetsAsync(QueryParametersDto query, CancellationToken ct = default)
    {
        var source = ApplyFilter(_repo.Query(), query.Filter);
        source = ApplySort(source, query.Sort);

        var total = source.Count();
        var items = source.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

        return await Task.FromResult(new PagedResponseDto<BudgetDto>
        {
            Data = items.Select(MapBudget).ToList(),
            TotalCount = total,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)query.PageSize)
        });
    }

    public async Task<BudgetDto?> GetBudgetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, ct);
        return entity is null ? null : MapBudget(entity);
    }

    private static IQueryable<Budget> ApplyFilter(IQueryable<Budget> source, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return source;
        var parts = filter.Split('=', 2);
        if (parts.Length != 2) return source;
        var key = parts[0].Trim().ToLowerInvariant();
        var value = parts[1].Trim();

        return key switch
        {
            "category" => source.Where(b => b.Category.Contains(value)),
            "projectid" when int.TryParse(value, out var pid) => source.Where(b => b.ProjectId == pid),
            _ => source
        };
    }

    private static IQueryable<Budget> ApplySort(IQueryable<Budget> source, string? sort)
    {
        var (field, desc) = ParseSort(sort);
        return field switch
        {
            "category" => desc ? source.OrderByDescending(b => b.Category) : source.OrderBy(b => b.Category),
            "allocatedamount" => desc ? source.OrderByDescending(b => b.AllocatedAmount) : source.OrderBy(b => b.AllocatedAmount),
            "spentamount" => desc ? source.OrderByDescending(b => b.SpentAmount) : source.OrderBy(b => b.SpentAmount),
            _ => source.OrderBy(b => b.Id)
        };
    }

    private static (string field, bool desc) ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return ("id", false);
        var parts = sort.Split(':', 2);
        var field = parts[0].Trim().ToLowerInvariant();
        var desc = parts.Length > 1 && parts[1].Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);
        return (field, desc);
    }

    private static BudgetDto MapBudget(Budget b) => new()
    {
        Id = b.Id,
        ProjectId = b.ProjectId,
        Category = b.Category,
        Description = b.Description,
        AllocatedAmount = b.AllocatedAmount,
        SpentAmount = b.SpentAmount,
        CreatedDate = b.CreatedDate,
        ModifiedDate = b.ModifiedDate,
        CostItems = b.CostItems.Select(c => new CostItemDto
        {
            Id = c.Id,
            BudgetId = c.BudgetId,
            Description = c.Description,
            Amount = c.Amount,
            Date = c.Date,
            Vendor = c.Vendor,
            Reference = c.Reference
        }).ToList()
    };
}
