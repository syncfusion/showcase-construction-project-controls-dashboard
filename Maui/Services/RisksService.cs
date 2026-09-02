using Construction.Maui.Models;

namespace Construction.Maui.Services;

/// <summary>Risks + Budgets endpoints mirror (RisksController + BudgetsController).</summary>
public interface IRisksService
{
    Task<PagedResponse<RiskDto>> GetRisksAsync(QueryParameters query, CancellationToken ct = default);
    Task<List<BudgetDto>> GetBudgetsAsync(QueryParameters query, CancellationToken ct = default);
}

public sealed class RisksService : IRisksService
{
    private readonly IApiClient _api;
    public RisksService(IApiClient api) => _api = api;

    public Task<PagedResponse<RiskDto>> GetRisksAsync(QueryParameters query, CancellationToken ct = default)
        => _api.GetJsonAsync<PagedResponse<RiskDto>>("risks" + query.ToQueryString(), ct);

    public Task<List<BudgetDto>> GetBudgetsAsync(QueryParameters query, CancellationToken ct = default)
        // Backend returns a PagedResponse<BudgetDto>; flatten to items for the grid view.
        => _api.GetJsonAsync<PagedResponse<BudgetDto>>("budgets" + query.ToQueryString(), ct)
            .ContinueWith(t => t.Result.Data, TaskContinuationOptions.ExecuteSynchronously);
}
