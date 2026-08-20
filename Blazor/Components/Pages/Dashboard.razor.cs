using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Charts;
using Syncfusion.Blazor.Grids;

namespace Construction.Blazor.Components.Pages;

public partial class Dashboard : ComponentBase
{
    [Inject] private ReportsService Reports { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ChartPaletteService Palette { get; set; } = default!;

    private PortfolioKpisDto? _portfolio;
    private CostKpisDto? _cost;
    private ProjectHealthDistributionDto? _health;
    private List<CostPerformancePointDto> _trend = [];
    private List<UpcomingMilestoneDto> _milestones = [];
    private bool _loading = true;
    private string? _error;
    private bool _paletteReady;

    // Syncfusion component refs — used to call .Refresh() after palette
    // resolution on first interactive render.
    private SfChart? _chart;
    private SfAccumulationChart? _donut;
    private SfGrid<UpcomingMilestoneDto>? _milestonesGrid;

    // Resolved at runtime from the in-house design tokens via ChartPaletteService.
    // Set inside OnAfterRenderAsync (first interactive render) — Syncfusion SVG
    // cannot read CSS variables, so we resolve :root to literal hex values.
    private ChartPrimaryXAxis _xAxis = new();
    private ChartPrimaryYAxis _yAxis = new();
    private ChartTooltipSettings _tooltip = new();

    private static readonly Dictionary<HealthStatus, string> HealthClasses = new()
    {
        [HealthStatus.NotStarted] = "text-secondary",
        [HealthStatus.OnTrack] = "positive",
        [HealthStatus.AtRisk] = "warning",
        [HealthStatus.Critical] = "negative",
    };

    private static readonly Dictionary<HealthStatus, string> HealthBadgeClass = new()
    {
        [HealthStatus.NotStarted] = "badge-neutral",
        [HealthStatus.OnTrack] = "badge-success",
        [HealthStatus.AtRisk] = "badge-warning",
        [HealthStatus.Critical] = "badge-error",
    };

    private record KpiSummaryItem(string Label, string Value, string Icon, string Trend, string Tone, string? To);

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var portfolioTask = Reports.GetPortfolioKpisAsync();
            var costTask = Reports.GetCostKpisAsync();
            var healthTask = Reports.GetProjectHealthDistributionAsync();
            var trendTask = Reports.GetCostPerformanceTrendAsync(6);
            var milestonesTask = Reports.GetUpcomingMilestonesAsync(14, 10);
            await Task.WhenAll(portfolioTask, costTask, healthTask, trendTask, milestonesTask);

            _portfolio = portfolioTask.Result;
            _cost = costTask.Result;
            _health = healthTask.Result;
            _trend = trendTask.Result;
            _milestones = milestonesTask.Result;
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_paletteReady)
        {
            await Palette.ResolveAsync();
            _xAxis = BuildXAxis();
            _yAxis = BuildYAxis();
            _tooltip = BuildTooltip();
            _paletteReady = true;
            StateHasChanged();
        }
    }

    private ChartPrimaryXAxis BuildXAxis() => new()
    {
        ValueType = Syncfusion.Blazor.Charts.ValueType.Category,
        Interval = 1,
        MajorGridLines = new ChartAxisMajorGridLines { Width = 0 },
        MajorTickLines = new ChartAxisMajorTickLines { Width = 0 },
        LineStyle = new ChartAxisLineStyle { Color = Palette.AxisLine, Width = 1 },
        LabelStyle = new ChartAxisLabelStyle
        {
            Color = Palette.AxisLabel,
            FontFamily = Palette.FontFamily,
            Size = Palette.CaptionSize,
            FontWeight = "500",
        },
    };

    private ChartPrimaryYAxis BuildYAxis() => new()
    {
        Visible = false,
        MajorGridLines = new ChartAxisMajorGridLines { Width = 0 },
        LineStyle = new ChartAxisLineStyle { Width = 0 },
        MajorTickLines = new ChartAxisMajorTickLines { Width = 0 },
    };

    private ChartTooltipSettings BuildTooltip() => new()
    {
        Enable = true,
        Shared = true,
        Fill = Palette.TooltipBg,
        Opacity = 1,
        Border = new ChartTooltipBorder { Width = 0 },
        TextStyle = new ChartTooltipTextStyle
        {
            Color = Palette.TooltipText,
            FontFamily = Palette.FontFamily,
            Size = "12px",
        },
    };

    // Donut series data — same shape as React/Angular ports.
    private List<HealthDonutSlice> HealthDonutData
    {
        get
        {
            if (_health is null) return [];
            var c = HealthCounts;
            var slices = new List<HealthDonutSlice>();
            if (c.OnTrack > 0) slices.Add(new() { X = "On track", Y = c.OnTrack, Color = Palette.OnTrack });
            if (c.AtRisk > 0) slices.Add(new() { X = "At risk", Y = c.AtRisk, Color = Palette.AtRisk });
            if (c.Critical > 0) slices.Add(new() { X = "Critical", Y = c.Critical, Color = Palette.Critical });
            if (c.NotStarted > 0) slices.Add(new() { X = "Not started", Y = c.NotStarted, Color = Palette.NotStarted });
            return slices;
        }
    }

    public class HealthDonutSlice
    {
        public string X { get; set; } = "";
        public double Y { get; set; }
        public string Color { get; set; } = "";
    }

    // Cost Performance Trend series data.
    private List<TrendPoint> TrendSeriesData =>
        _trend.Select(p => new TrendPoint { X = p.Month, Planned = (double)p.Planned, Actual = (double)p.Actual }).ToList();

    public class TrendPoint
    {
        public string X { get; set; } = "";
        public double Planned { get; set; }
        public double Actual { get; set; }
    }

    private (int OnTrack, int AtRisk, int Critical, int NotStarted, int Total) HealthCounts =>
        _health is null
            ? (0, 0, 0, 0, 0)
            : (_health.OnTrack, _health.AtRisk, _health.Critical, _health.NotStarted,
               _health.OnTrack + _health.AtRisk + _health.Critical + _health.NotStarted);

    private static string FormatPct(decimal n) => $"{(n >= 0 ? "+" : "")}{n:0.0}%";

    private List<KpiSummaryItem>? KpiSummary
    {
        get
        {
            if (_portfolio is null || _cost is null) return null;
            var p = _portfolio;
            return
            [
                new("Active Projects", p.ActiveProjects.ToString(), "arrow-up-right", "3 this quarter", "positive", "/projects"),
                new("Schedule Variance (SV)", FormatPct(p.ScheduleVariancePct), p.ScheduleVariancePct >= 0 ? "arrow-up-right" : "arrow-down-right",
                    $"{Math.Abs(p.ScheduleVariancePct):0.0}% {(p.ScheduleVariancePct >= 0 ? "ahead" : "behind")} plan", p.ScheduleVariancePct >= 0 ? "positive" : "negative", null),
                new("Cost Variance (CV)", FormatPct(p.CostVariancePct), p.CostVariancePct >= 0 ? "arrow-up-right" : "arrow-down-right",
                    p.CostVariancePct >= 0 ? "Under budget" : "Over budget", p.CostVariancePct >= 0 ? "positive" : "negative", "/cost-control"),
                new("CPI", p.Cpi.ToString("0.00"), p.Cpi >= 1 ? "arrow-up-right" : "alert-circle", p.Cpi >= 1 ? "On target" : "Below target", p.Cpi >= 1 ? "positive" : "warning", null),
                new("SPI", p.Spi.ToString("0.00"), p.Spi >= 1 ? "arrow-up-right" : "arrow-down-right", p.Spi >= 1 ? "Schedule on track" : "Recovery needed", p.Spi >= 1 ? "positive" : "negative", null),
                new("Open Risks", p.OpenRisks.ToString(), p.CriticalRisks > 0 ? "alert-triangle" : "check-circle", $"{p.CriticalRisks} critical", p.CriticalRisks > 0 ? "negative" : "positive", "/risks"),
            ];
        }
    }

    private void GoTo(string path) => Nav.NavigateTo(path);

    private void OnKpiKeydown(KeyboardEventArgs e, KpiSummaryItem kpi)
    {
        if (kpi.To is null) return;
        KeyboardActivation.OnActivateKey(e, () => GoTo(kpi.To));
    }

    private static string SplitHealthLabel(HealthStatus status) =>
        System.Text.RegularExpressions.Regex.Replace(status.ToString(), "([A-Z])", " $1").Trim();
}
