using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Construction.Maui.Models;
using Construction.Maui.Services;
using Construction.Maui.ViewModels.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Brush = Microsoft.Maui.Controls.Brush;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;

namespace Construction.Maui.ViewModels;

/// <summary>
/// Dashboard page ViewModel — mirrors React/Blazor Dashboard data binding:
/// portfolio KPIs + cost KPIs + project health distribution + 6-month cost
/// performance trend + upcoming milestones (14-day horizon, top 10).
/// </summary>
public sealed partial class DashboardViewModel : ViewModelBase
{
    private readonly IReportsService _reports;

    public DashboardViewModel(IReportsService reports) => _reports = reports;

    [ObservableProperty] private PortfolioKpisDto? _portfolio;
    [ObservableProperty] private CostKpisDto? _cost;
    [ObservableProperty] private ProjectHealthDistributionDto? _health;
    [ObservableProperty] private UpcomingMilestoneDto? _hoveredMilestone;

    public ObservableCollection<UpcomingMilestoneDto> Milestones { get; } = new();
    public ObservableCollection<CostPerformancePointDto> Trend { get; } = new();
    public ObservableCollection<ChartSegmentVm> HealthSegments { get; } = new();

    public List<Brush> HealthPalette { get; } = new()
    {
        new SolidColorBrush(Color.FromArgb("#039855")),
        new SolidColorBrush(Color.FromArgb("#DC6803")),
        new SolidColorBrush(Color.FromArgb("#D92C20")),
        new SolidColorBrush(Color.FromArgb("#EAECF0")),
    };

    public override bool HasData => Portfolio is not null && Cost is not null;

    public override async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var portfolio = _reports.GetPortfolioKpisAsync();
            var cost = _reports.GetCostKpisAsync();
            var health = _reports.GetProjectHealthDistributionAsync();
            var trend = _reports.GetCostPerformanceTrendAsync(6);
            var mst = _reports.GetUpcomingMilestonesAsync(14, 10);
            await Task.WhenAll(portfolio, cost, health, trend, mst);

            var portfolioResult = portfolio.Result;
            var costResult = cost.Result;
            var healthResult = health.Result;
            var trendResult = trend.Result;
            var milestonesResult = mst.Result;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Portfolio = portfolioResult;
                Cost = costResult;
                Health = healthResult;

                Trend.Clear();
                foreach (var p in trendResult) Trend.Add(p);

                Milestones.Clear();
                foreach (var m in milestonesResult) Milestones.Add(m);

                HealthSegments.Clear();
                if (healthResult is { Total: > 0 })
                {
                    HealthSegments.Add(new ChartSegmentVm { Label = "On Track",    Value = (double)healthResult.OnTrack    / healthResult.Total * 100 });
                    HealthSegments.Add(new ChartSegmentVm { Label = "At Risk",     Value = (double)healthResult.AtRisk     / healthResult.Total * 100 });
                    HealthSegments.Add(new ChartSegmentVm { Label = "Critical",    Value = (double)healthResult.Critical   / healthResult.Total * 100 });
                    HealthSegments.Add(new ChartSegmentVm { Label = "Not Started", Value = (double)healthResult.NotStarted / healthResult.Total * 100 });
                }
            });
        });
    }

    // ---- KPI display projections ----

    public string ScheduleVarianceDisplay =>
        Portfolio is null ? "—" : $"{(Portfolio.ScheduleVariancePct >= 0 ? "+" : "")}{Portfolio.ScheduleVariancePct:0.0}%";

    public string CostVarianceDisplay =>
        Portfolio is null ? "—" : $"{(Portfolio.CostVariancePct >= 0 ? "+" : "")}{Portfolio.CostVariancePct:0.0}%";

    public string ScheduleVarianceTrend =>
        Portfolio is null ? "—" : $"{Math.Abs(Portfolio.ScheduleVariancePct):0.0}% {(Portfolio.ScheduleVariancePct >= 0 ? "ahead" : "behind")} plan";

    public string CostVarianceTrend =>
        Portfolio is null ? "—" : (Portfolio.CostVariancePct >= 0 ? "Under budget" : "Over budget");

    public string CpiDisplay => Portfolio is null ? "—" : $"{Portfolio.Cpi:0.00}";
    public string SpiDisplay => Portfolio is null ? "—" : $"{Portfolio.Spi:0.00}";

    // Trends for the React-parity KPI cards (CPI / SPI / Open Risks). The tone
    // (positive/negative/warning) drives the trend label color in the XAML via
    // AppThemeBinding so light/dark themes both render correctly.
    public string CpiTrend =>
        Portfolio is null ? "—" : (Portfolio.Cpi >= 1 ? "On target" : "Below target");
    public bool CpiIsPositive => Portfolio is null || Portfolio.Cpi >= 1;

    public string SpiTrend =>
        Portfolio is null ? "—" : (Portfolio.Spi >= 1 ? "Schedule on track" : "Recovery needed");
    public bool SpiIsPositive => Portfolio is null || Portfolio.Spi >= 1;

    public string OpenRisksTrend => Portfolio is null ? "—" : $"{Portfolio.CriticalRisks} critical";
    public bool OpenRisksIsCritical => Portfolio is { CriticalRisks: > 0 };

    public string TotalBudgetDisplay        => Cost is null ? "—" : Cost.TotalPortfolioBudget.ToString("C0");
    public string CommittedSpendDisplay     => Cost is null ? "—" : Cost.CommittedSpend.ToString("C0");
    public string ForecastAtCompletionDisplay => Cost is null ? "—" : Cost.ForecastAtCompletion.ToString("C0");

    public int TotalProjects      => Health?.Total ?? 0;
    public double OnTrackShare    => Health is null || Health.Total == 0 ? 0 : (double)Health.OnTrack   / Health.Total * 100;
    public double AtRiskShare     => Health is null || Health.Total == 0 ? 0 : (double)Health.AtRisk    / Health.Total * 100;
    public double CriticalShare   => Health is null || Health.Total == 0 ? 0 : (double)Health.Critical  / Health.Total * 100;
    public double NotStartedShare => Health is null || Health.Total == 0 ? 0 : (double)Health.NotStarted/ Health.Total * 100;

    public string HealthLegend => Health is null
        ? "—"
        : $"{Health.OnTrack} On Track · {Health.AtRisk} At Risk · {Health.Critical} Critical · {Health.NotStarted} Not Started";

    // Re-raise display props when source changes
    partial void OnPortfolioChanged(PortfolioKpisDto? value)
    {
        // HasData depends on both Portfolio and Cost — re-notify so the
        // IsVisible="{Binding HasData}" grid + charts update from their
        // collapsed initial state once the first source lands.
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(ScheduleVarianceDisplay));
        OnPropertyChanged(nameof(CostVarianceDisplay));
        OnPropertyChanged(nameof(ScheduleVarianceTrend));
        OnPropertyChanged(nameof(CostVarianceTrend));
        OnPropertyChanged(nameof(CpiDisplay));
        OnPropertyChanged(nameof(SpiDisplay));
        // React-parity KPI card props (CPI/SPI/OpenRisks trends + tone booleans)
        OnPropertyChanged(nameof(CpiTrend));
        OnPropertyChanged(nameof(CpiIsPositive));
        OnPropertyChanged(nameof(SpiTrend));
        OnPropertyChanged(nameof(SpiIsPositive));
        OnPropertyChanged(nameof(OpenRisksTrend));
        OnPropertyChanged(nameof(OpenRisksIsCritical));
    }
    partial void OnCostChanged(CostKpisDto? value)
    {
        // See OnPortfolioChanged — HasData also depends on Cost.
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(TotalBudgetDisplay));
        OnPropertyChanged(nameof(CommittedSpendDisplay));
        OnPropertyChanged(nameof(ForecastAtCompletionDisplay));
    }
    partial void OnHealthChanged(ProjectHealthDistributionDto? value)
    {
        OnPropertyChanged(nameof(TotalProjects));
        OnPropertyChanged(nameof(OnTrackShare));
        OnPropertyChanged(nameof(AtRiskShare));
        OnPropertyChanged(nameof(CriticalShare));
        OnPropertyChanged(nameof(NotStartedShare));
        OnPropertyChanged(nameof(HealthLegend));
    }
}
