using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Construction.Maui.Models;

namespace Construction.Maui.Services;

/// <summary>
/// Reads the pre-extracted JSON data files from the <c>Data/</c> folder so the
/// showcase dashboard renders with real data WITHOUT requiring the backend API
/// (or any DB) to be running at runtime. The JSON files were extracted from the
/// backend DB via the API seeder (see the <c>Data/</c> folder + the copy targets
/// in <c>Construction.Maui.csproj</c>).
///
/// Resolution mirrors <c>MauiProgram.cs</c>'s appsettings lookup:
///   • Unpackaged Windows: probe <c>AppContext.BaseDirectory</c> (next to the .exe)
///     first — the <c>CopyDataOnBuild</c> target copies <c>Data/</c> there.
///   • Packaged (Android/iOS/MSIX): fall back to <c>FileSystem.OpenAppPackageFileAsync</c>,
///     where the <c>&lt;MauiAsset Include="Data\**"&gt;</c> entry ships the files.
///
/// All public methods are async and mirror the shape the original HTTP services
/// returned (PagedResponse&lt;T&gt;, List&lt;T&gt;, or a single DTO) so the
/// ViewModels can keep their existing binding code unchanged.
/// </summary>
public interface ILocalDataService
{
    Task<PortfolioKpisDto> GetPortfolioKpisAsync(CancellationToken ct = default);
    Task<CostKpisDto> GetCostKpisAsync(CancellationToken ct = default);
    Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync(CancellationToken ct = default);
    Task<List<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months = 12, CancellationToken ct = default);
    Task<List<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months = 12, CancellationToken ct = default);
    Task<List<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync(CancellationToken ct = default);
    Task<List<UpcomingMilestoneDto>> GetPortfolioUpcomingMilestonesAsync(int days = 365, int limit = 200, CancellationToken ct = default);

    Task<PagedResponse<ProjectDto>> GetProjectsAsync(QueryParameters query, CancellationToken ct = default);
    Task<ProjectDto?> GetProjectAsync(int id, CancellationToken ct = default);
    Task<ProjectKpisDto?> GetProjectKpisAsync(int id, CancellationToken ct = default);
    Task<List<TaskDto>> GetProjectTasksAsync(int id, QueryParameters query, CancellationToken ct = default);
    Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days = 365, int limit = 50, CancellationToken ct = default);
    Task<List<RiskDto>> GetTopOpenRisksAsync(int id, int limit = 20, CancellationToken ct = default);
    Task<List<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days = 365, CancellationToken ct = default);
    Task<List<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default);
    Task<List<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default);
    Task<List<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default);

    Task<PagedResponse<RiskDto>> GetRisksAsync(QueryParameters query, CancellationToken ct = default);

    Task<List<BudgetDto>> GetBudgetsAsync(QueryParameters query, CancellationToken ct = default);
}

public sealed class LocalDataService : ILocalDataService
{
    private readonly JsonSerializerOptions _json;

    public LocalDataService(JsonSerializerOptions json) => _json = json;

    // ---- File resolution ------------------------------------------------------

    /// <summary>Opens a JSON data file by its path relative to the <c>Data/</c>
    /// folder. Resolution order:
    ///   1. <c>AppContext.BaseDirectory/Data/</c> (unpackaged Windows — the
    ///      CopyDataOnBuild target placed the folder next to the .exe; this is
    ///      also where a manual deploy would put it).
    ///   2. Walk UP from <c>AppContext.BaseDirectory</c> looking for a
    ///      <c>Data/</c> folder — covers dev runs from bin when the copy target
    ///      didn't fire (the project-source <c>Data/</c> is reached via the
    ///      traversal since bin sits under the project root).
    ///   3. <c>FileSystem.OpenAppPackageFileAsync("Data/&lt;path&gt;")</c>
    ///      (packaged Android/iOS/MSIX runs — the &lt;MauiAsset&gt; entry ships
    ///      the files inside the app package).</summary>
    private async Task<Stream> OpenDataAsync(string relativePath, CancellationToken ct)
    {
        // 1) Probe next to the .exe (unpackaged Windows, post-copy or manual deploy).
        var exePath = Path.Combine(AppContext.BaseDirectory, "Data", relativePath);
        if (File.Exists(exePath))
            return File.OpenRead(exePath);

        // 2) Walk up from BaseDirectory to find a Data/ folder (dev fallback when
        //    the bin copy target didn't run — reaches the project-source Data/ via
        //    bin/Debug/net10.0-windows10.0.19041.0/win-x64 → ... → project root).
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "Data", relativePath);
            if (File.Exists(candidate))
                return File.OpenRead(candidate);
            dir = dir.Parent;
        }

        // 3) Packaged runs — the <MauiAsset Include="Data\**"> entry bundles the
        //    files into the app package; FileSystem.OpenAppPackageFileAsync reads
        //    them relative to the package root.
        var bundledPath = Path.Combine("Data", relativePath);
        return await FileSystem.OpenAppPackageFileAsync(bundledPath).ConfigureAwait(false);
    }

    private async Task<T> ReadJsonAsync<T>(string relativePath, CancellationToken ct)
    {
        await using var stream = await OpenDataAsync(relativePath, ct).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync<T>(stream, _json, ct).ConfigureAwait(false))
            ?? throw new InvalidDataException($"Deserialized null from Data/{relativePath}");
    }

    // ---- Portfolio / reports (singletons) ------------------------------------

    public Task<PortfolioKpisDto> GetPortfolioKpisAsync(CancellationToken ct = default)
        => ReadJsonAsync<PortfolioKpisDto>("reports-portfolio-kpis.json", ct);

    public Task<CostKpisDto> GetCostKpisAsync(CancellationToken ct = default)
        => ReadJsonAsync<CostKpisDto>("reports-cost-kpis.json", ct);

    public Task<ProjectHealthDistributionDto> GetProjectHealthDistributionAsync(CancellationToken ct = default)
        => ReadJsonAsync<ProjectHealthDistributionDto>("reports-project-health-distribution.json", ct);

    public Task<List<CostPerformancePointDto>> GetCostPerformanceTrendAsync(int months = 12, CancellationToken ct = default)
        => ReadJsonAsync<List<CostPerformancePointDto>>("reports-cost-performance-trend.json", ct);

    public Task<List<EarnedValuePointDto>> GetEarnedValueTrendAsync(int months = 12, CancellationToken ct = default)
        => ReadJsonAsync<List<EarnedValuePointDto>>("reports-earned-value-trend.json", ct);

    public Task<List<CostVarianceByCostCodeDto>> GetCostVarianceByCostCodeAsync(CancellationToken ct = default)
        => ReadJsonAsync<List<CostVarianceByCostCodeDto>>("reports-cost-variance-by-cost-code.json", ct);

    public async Task<List<UpcomingMilestoneDto>> GetPortfolioUpcomingMilestonesAsync(int days = 365, int limit = 200, CancellationToken ct = default)
    {
        var all = await ReadJsonAsync<List<UpcomingMilestoneDto>>("reports-upcoming-milestones.json", ct).ConfigureAwait(false);
        return all;
    }

    // ---- Projects (list + per-project detail) --------------------------------

    public async Task<PagedResponse<ProjectDto>> GetProjectsAsync(QueryParameters query, CancellationToken ct = default)
    {
        var all = await ReadJsonAsync<PagedResponse<ProjectDto>>("projects.json", ct).ConfigureAwait(false);
        // Re-apply the client-side status filter the original service applied server-side.
        var data = all.Data;
        if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "All", StringComparison.OrdinalIgnoreCase))
            data = data.Where(p => string.Equals(p.Status.ToString(), query.Status, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var q = query.Search.Trim();
            data = data.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                                || p.Code.Contains(q, StringComparison.OrdinalIgnoreCase)
                                || (p.Location ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return new PagedResponse<ProjectDto>
        {
            Data = data,
            TotalCount = data.Count,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = query.PageSize > 0 ? (int)Math.Ceiling(data.Count / (double)query.PageSize) : 1,
        };
    }

    public async Task<ProjectDto?> GetProjectAsync(int id, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, ProjectDto>>("projects/by-id.json", ct).ConfigureAwait(false);
        return byId.TryGetValue(id.ToString(), out var p) ? p : null;
    }

    public async Task<ProjectKpisDto?> GetProjectKpisAsync(int id, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, ProjectKpisDto>>("projects/kpis-by-id.json", ct).ConfigureAwait(false);
        return byId.TryGetValue(id.ToString(), out var k) ? k : null;
    }

    public async Task<List<TaskDto>> GetProjectTasksAsync(int id, QueryParameters query, CancellationToken ct = default)
    {
        // Tasks weren't individually extracted (the Schedule tab data) — return an
        // empty list. The Schedule tab degrades gracefully to "no tasks".
        return new List<TaskDto>();
    }

    public async Task<List<RecentDocumentDto>> GetRecentDocumentsAsync(int id, int days = 365, int limit = 50, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, List<RecentDocumentDto>>>("projects/recent-documents-by-id.json", ct).ConfigureAwait(false);
        if (!byId.TryGetValue(id.ToString(), out var docs)) return new List<RecentDocumentDto>();
        return docs.Take(limit).ToList();
    }

    public async Task<List<RiskDto>> GetTopOpenRisksAsync(int id, int limit = 20, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, List<RiskDto>>>("projects/top-open-risks-by-id.json", ct).ConfigureAwait(false);
        if (!byId.TryGetValue(id.ToString(), out var risks)) return new List<RiskDto>();
        return risks.Take(limit).ToList();
    }

    public async Task<List<MilestoneDto>> GetUpcomingMilestonesAsync(int id, int days = 365, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, List<MilestoneDto>>>("projects/upcoming-milestones-by-id.json", ct).ConfigureAwait(false);
        if (!byId.TryGetValue(id.ToString(), out var ms)) return new List<MilestoneDto>();
        return ms;
    }

    public async Task<List<RfiSummaryDto>> GetProjectRfisAsync(int id, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, List<RfiSummaryDto>>>("projects/rfis-by-id.json", ct).ConfigureAwait(false);
        if (!byId.TryGetValue(id.ToString(), out var rfis)) return new List<RfiSummaryDto>();
        return rfis;
    }

    public async Task<List<SubmittalSummaryDto>> GetProjectSubmittalsAsync(int id, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, List<SubmittalSummaryDto>>>("projects/submittals-by-id.json", ct).ConfigureAwait(false);
        if (!byId.TryGetValue(id.ToString(), out var subs)) return new List<SubmittalSummaryDto>();
        return subs;
    }

    public async Task<List<ChangeOrderSummaryDto>> GetProjectChangeOrdersAsync(int id, CancellationToken ct = default)
    {
        var byId = await ReadJsonAsync<Dictionary<string, List<ChangeOrderSummaryDto>>>("projects/change-orders-by-id.json", ct).ConfigureAwait(false);
        if (!byId.TryGetValue(id.ToString(), out var cos)) return new List<ChangeOrderSummaryDto>();
        return cos;
    }

    // ---- Risks (portfolio) ----------------------------------------------------

    public async Task<PagedResponse<RiskDto>> GetRisksAsync(QueryParameters query, CancellationToken ct = default)
    {
        var all = await ReadJsonAsync<PagedResponse<RiskDto>>("risks.json", ct).ConfigureAwait(false);
        var data = all.Data;
        if (!string.IsNullOrWhiteSpace(query.Status) && !string.Equals(query.Status, "All", StringComparison.OrdinalIgnoreCase))
            data = data.Where(r => string.Equals(r.Status.ToString(), query.Status, StringComparison.OrdinalIgnoreCase)).ToList();
        return new PagedResponse<RiskDto>
        {
            Data = data,
            TotalCount = data.Count,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = query.PageSize > 0 ? (int)Math.Ceiling(data.Count / (double)query.PageSize) : 1,
        };
    }

    // ---- Budgets (cost control) -----------------------------------------------
    public async Task<List<BudgetDto>> GetBudgetsAsync(QueryParameters query, CancellationToken ct = default)
    {
        var paged = await ReadJsonAsync<PagedResponse<BudgetDto>>("budgets.json", ct).ConfigureAwait(false);
        return paged.Data;
    }
}
