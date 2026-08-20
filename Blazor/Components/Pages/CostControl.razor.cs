using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Charts;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.HeatMap;

namespace Construction.Blazor.Components.Pages;

public partial class CostControl : ComponentBase
{
    [Inject] private ReportsService Reports { get; set; } = default!;
    [Inject] private ChangeOrdersService ChangeOrdersApi { get; set; } = default!;
    [Inject] private DownloadInterop Download { get; set; } = default!;
    [Inject] private ChartPaletteService Palette { get; set; } = default!;

    private static readonly CostVarianceByCostCodeDto[] FallbackCostCodes =
    [
        new() { CostCode = "General Conditions", VariancePct = 2 },
        new() { CostCode = "Sitework", VariancePct = 4 },
        new() { CostCode = "Concrete", VariancePct = -3 },
        new() { CostCode = "Masonry", VariancePct = -5 },
        new() { CostCode = "Metals", VariancePct = -8 },
        new() { CostCode = "Finishes", VariancePct = 1 },
    ];

    private static readonly Dictionary<ChangeOrderStatus, string> StatusBadgeClass = new()
    {
        [ChangeOrderStatus.Draft] = "badge-neutral",
        [ChangeOrderStatus.Pending] = "badge-warning",
        [ChangeOrderStatus.Approved] = "badge-success",
        [ChangeOrderStatus.Rejected] = "badge-error",
        [ChangeOrderStatus.Implemented] = "badge-success",
    };

    private static readonly (ChangeOrderStatus? Status, string Label)[] StatusOptions =
    [
        (null, "All statuses"), (ChangeOrderStatus.Draft, "Draft"), (ChangeOrderStatus.Pending, "Pending"),
        (ChangeOrderStatus.Approved, "Approved"), (ChangeOrderStatus.Rejected, "Rejected"), (ChangeOrderStatus.Implemented, "Implemented"),
    ];

    private CostKpisDto? _kpis;
    private List<CostPerformancePointDto> _trend = [];
    private List<CostVarianceByCostCodeDto> _variances = [];
    private List<ChangeOrderSummaryDto> _changeOrders = [];
    private string _coSearch = "";
    private ChangeOrderStatus? _coStatus = ChangeOrderStatus.Pending;
    private bool _coLoading = true;
    private bool _loading = true;
    private string? _error;
    private ChangeOrderSummaryDto? _selectedCo;
    private bool _showNewCoModal;
    private NewChangeOrderDraft _draft = new();

    // Syncfusion component state — see Dashboard for the full pattern.
    private bool _paletteReady;
    private SfChart? _chart;
    private SfHeatMap<object>? _heatmap;
    private SfGrid<ChangeOrderSummaryDto>? _coGrid;

    private class NewChangeOrderDraft
    {
        public string ProjectId { get; set; } = "";
        public string Description { get; set; } = "";
        public string Amount { get; set; } = "";
        public string RequestedBy { get; set; } = "";
        public DateTime RequestDate { get; set; } = DateTime.Today;
        public string ImpactDays { get; set; } = "";
        public string Justification { get; set; } = "";
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var kpisTask = Reports.GetCostKpisAsync();
            var trendTask = Reports.GetCostPerformanceTrendAsync(7);
            var varianceTask = Reports.GetCostVarianceByCostCodeAsync();
            await Task.WhenAll(kpisTask, trendTask, varianceTask);
            _kpis = kpisTask.Result;
            _trend = trendTask.Result;
            _variances = varianceTask.Result.Count > 0 ? varianceTask.Result : FallbackCostCodes.ToList();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }

        try
        {
            var result = await ChangeOrdersApi.GetChangeOrdersAsync(1, 1000);
            _changeOrders = result.Data.ToList();
        }
        catch
        {
            _changeOrders = [];
        }
        finally
        {
            _coLoading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_paletteReady)
        {
            await Palette.ResolveAsync();
            _paletteReady = true;
            StateHasChanged();
        }
    }

    private decimal? BudgetDelta => _kpis is null ? null : _kpis.TotalPortfolioBudget - _kpis.ForecastAtCompletion;

    private static string PctOfBudget(decimal spend, decimal budget) => budget == 0 ? "—" : $"{Math.Round(spend / budget * 100)}% of budget";

    // Cost Performance Trend series data — fed into <SfChart> Column series.
    private List<TrendPoint> TrendSeriesData =>
        _trend.Select(p => new TrendPoint { X = p.Month, Planned = (double)p.Planned, Actual = (double)p.Actual }).ToList();

    public class TrendPoint
    {
        public string X { get; set; } = "";
        public double Planned { get; set; }
        public double Actual { get; set; }
    }

    // Cost Variance HeatMap data — a 1×N `double[,]` 2D array. The Blazor
    // HeatMap's DataSource property is `object`, and the Table adaptor (the
    // default) expects a 2D array of numeric values. We use 0/1/2 tone
    // scores and let CellRendering paint the actual background colour per
    // cell using the in-house token palette.
    private object HeatmapData => BuildHeatmapData();

    private double[,] BuildHeatmapData()
    {
        var n = _variances.Count;
        var data = new double[1, Math.Max(1, n)];
        for (int i = 0; i < n; i++)
        {
            data[0, i] = VarianceToneScore(_variances[i].VariancePct);
        }
        return data;
    }

    private static int VarianceToneScore(decimal variancePct) => variancePct >= 0 ? 0 : variancePct >= -5 ? 1 : 2;

    private string[] HeatmapXAxisLabels => _variances.Select(v => v.CostCode).ToArray();

    private List<ChangeOrderSummaryDto> FilteredChangeOrders =>
        _changeOrders.Where(co =>
        {
            var matchesStatus = _coStatus is null || co.Status == _coStatus;
            var q = _coSearch.Trim();
            var matchesSearch = string.IsNullOrEmpty(q)
                || co.Number.Contains(q, StringComparison.OrdinalIgnoreCase)
                || co.Description.Contains(q, StringComparison.OrdinalIgnoreCase)
                || co.ProjectId.ToString().Contains(q, StringComparison.OrdinalIgnoreCase);
            return matchesStatus && matchesSearch;
        }).ToList();

    private void OnSearchChanged(ChangeEventArgs e) => _coSearch = e.Value?.ToString() ?? "";

    private void OnStatusChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString();
        _coStatus = string.IsNullOrEmpty(value) ? null : Enum.Parse<ChangeOrderStatus>(value);
    }

    private void OpenNewChangeOrderModal()
    {
        _draft = new NewChangeOrderDraft();
        _showNewCoModal = true;
    }

    private void SaveNewChangeOrder()
    {
        if (string.IsNullOrWhiteSpace(_draft.Description)) return;
        var nextId = _changeOrders.Count > 0 ? _changeOrders.Max(c => c.Id) + 1 : 1;
        var created = new ChangeOrderSummaryDto
        {
            Id = nextId,
            ProjectId = int.TryParse(_draft.ProjectId, out var pid) ? pid : 0,
            Number = $"CO-{nextId:D4}",
            Description = _draft.Description.Trim(),
            Amount = decimal.TryParse(_draft.Amount, out var amt) ? amt : 0,
            Status = ChangeOrderStatus.Draft,
            RequestedBy = string.IsNullOrWhiteSpace(_draft.RequestedBy) ? null : _draft.RequestedBy.Trim(),
            RequestDate = _draft.RequestDate,
        };
        // Demo only: kept in local component state so it's visible in the UI immediately;
        // nothing is written back to the API.
        _changeOrders = [created, .. _changeOrders];
        _coStatus = null;
        _coSearch = "";
        _showNewCoModal = false;
    }

    private async Task ExportChangeOrders()
    {
        var csv = CsvBuilder.Build<ChangeOrderSummaryDto>(
        [
            new("CO #", co => co.Number),
            new("Project ID", co => co.ProjectId),
            new("Description", co => co.Description),
            new("Submitted", co => co.RequestDate is not null ? Formatters.FormatDate(co.RequestDate) : ""),
            new("Amount", co => co.Amount),
            new("Status", co => co.Status),
        ], FilteredChangeOrders);
        await Download.DownloadTextFileAsync("change-orders.csv", "text/csv;charset=utf-8;", csv);
    }

    // Syncfusion grid rowSelected handler — opens the change-order modal.
    private void OnCoRowSelected(RowSelectEventArgs<ChangeOrderSummaryDto> args)
    {
        if (args.Data is not null) _selectedCo = args.Data;
    }

    // HeatMap CellRendering handler — forces the cell colour to the matching
    // tone background so the per-cell palette is bound to the variance
    // bucket, not to the numeric value. The Blazor HeatMapCellRenderEventArgs
    // in v34.1.29 exposes CellColor for paint customization; the label text
    // is left as the cell's default rendered value (the tone score 0/1/2),
    // which keeps the data shape clean. The visible cost-code + variance
    // context comes from the X-axis label and the cell background colour.
    private void OnHeatmapCellRender(HeatMapCellRenderEventArgs args)
    {
        // Find the variance that matches the cell's x-label.
        var match = _variances.FirstOrDefault(v => v.CostCode == args.XLabel);
        if (match is null) return;
        var v = match.VariancePct;
        args.CellColor = v >= 0 ? Palette.PositiveBg : v >= -5 ? Palette.WarningBg : Palette.NegativeBg;
    }
}
