using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Construction.Maui.Models;
using Construction.Maui.Services;
using Construction.Maui.ViewModels.Base;

namespace Construction.Maui.ViewModels;

/// <summary>
/// Cost Control page ViewModel — mirrors React/Blazor CostControlPage:
/// portfolio cost KPIs + earned-value trend (BCWS/BCWP/ACWP) + budgets grid
/// (allocated/spent/remaining/% used) + cost-variance-by-cost-code bar chart.
/// All data fetched from the existing Reports + Budgets endpoints (no static data).
/// </summary>
public sealed partial class CostControlViewModel : ViewModelBase
{
    private readonly IReportsService _reports;
    private readonly IRisksService _risks;

    public CostControlViewModel(IReportsService reports, IRisksService risks)
    {
        _reports = reports;
        _risks = risks;
    }

    [ObservableProperty] private CostKpisDto? _costKpis;
    [ObservableProperty] private PortfolioKpisDto? _portfolio;

    public ObservableCollection<EarnedValuePointDto> EarnedValueTrend { get; } = new();
    public ObservableCollection<CostVarianceByCostCodeDto> VarianceByCode { get; } = new();
    public ObservableCollection<BudgetDto> Budgets { get; } = new();

    public override bool HasData => CostKpis is not null;

    public override async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var ck        = _reports.GetCostKpisAsync();
            var pk        = _reports.GetPortfolioKpisAsync();
            var ev        = _reports.GetEarnedValueTrendAsync(6);
            var vc        = _reports.GetCostVarianceByCostCodeAsync();
            var budgets   = _risks.GetBudgetsAsync(new QueryParameters { Page = 1, PageSize = 100 });
            await Task.WhenAll(ck, pk, ev, vc, budgets);

            // Capture on the (threadpool) continuation, then mutate the
            // ObservableProperty + ObservableCollection bindings on the UI
            // thread — MAUI/WinUI silently ignores IsVisible/ItemsSource
            // binding updates raised off the UI thread (the await resumes
            // off-thread because LocalDataService uses ConfigureAwait(false)).
            // Mirrors the DashboardViewModel fix.
            var costKpis = ck.Result;
            var portfolio = pk.Result;
            var evList = ev.Result;
            var vcList = vc.Result;
            var budgetList = budgets.Result;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CostKpis = costKpis;
                Portfolio = portfolio;

                EarnedValueTrend.Clear();
                foreach (var x in evList) EarnedValueTrend.Add(x);

                VarianceByCode.Clear();
                foreach (var x in vcList) VarianceByCode.Add(x);

                Budgets.Clear();
                foreach (var x in budgetList) Budgets.Add(x);
            });
        });
    }

    // ---- Display projections ----
    public string TotalBudgetDisplay          => CostKpis is null ? "—" : CostKpis.TotalPortfolioBudget.ToString("C0");
    public string CommittedSpendDisplay        => CostKpis is null ? "—" : CostKpis.CommittedSpend.ToString("C0");
    public string ForecastAtCompletionDisplay  => CostKpis is null ? "—" : CostKpis.ForecastAtCompletion.ToString("C0");
    public string PendingChangeOrdersDisplay   => CostKpis is null ? "—" : CostKpis.PendingChangeOrdersAmount.ToString("C0");
    public string CpiDisplay                   => Portfolio is null ? "—" : $"{Portfolio.Cpi:0.00}";
    public string SpiDisplay                   => Portfolio is null ? "—" : $"{Portfolio.Spi:0.00}";

    // HasData depends on CostKpis; re-notify so IsVisible="{Binding HasData}"
    // sections (KPI grid, charts, budgets grid) update when the source lands.
    partial void OnCostKpisChanged(CostKpisDto? value)
    {
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(TotalBudgetDisplay));
        OnPropertyChanged(nameof(CommittedSpendDisplay));
        OnPropertyChanged(nameof(ForecastAtCompletionDisplay));
        OnPropertyChanged(nameof(PendingChangeOrdersDisplay));
    }
    partial void OnPortfolioChanged(PortfolioKpisDto? value)
    {
        OnPropertyChanged(nameof(CpiDisplay));
        OnPropertyChanged(nameof(SpiDisplay));
    }
}
