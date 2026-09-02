using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Construction.Maui.Models;
using Construction.Maui.Services;
using Construction.Maui.ViewModels.Base;

namespace Construction.Maui.ViewModels;

/// <summary>
/// Reports page ViewModel — mirrors the React/Blazor ReportsPage: an Earned
/// Value Trend multi-series chart (BCWS/BCWP/ACWP over 12 months) and a Cost
/// Variance by Cost Code column chart. Both come from <see cref="IReportsService"/>'s
/// <c>earned-value-trend</c> and <c>cost-variance-by-cost-code</c> endpoints.
/// </summary>
public sealed partial class ReportsViewModel : ViewModelBase
{
    private readonly IReportsService _reports;

    public ReportsViewModel(IReportsService reports) => _reports = reports;

    /// <summary>Single 12-month series the EV chart binds to (one point per month).</summary>
    public ObservableCollection<EarnedValuePointDto> EarnedValueTrend { get; } = new();

    /// <summary>Cost-code variance series the CV chart binds to.</summary>
    public ObservableCollection<CostVarianceByCostCodeDto> CostVarianceByCode { get; } = new();

    public override bool HasData => EarnedValueTrend.Count > 0 || CostVarianceByCode.Count > 0;

    public override async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var ev  = _reports.GetEarnedValueTrendAsync(12);
            var cvc = _reports.GetCostVarianceByCostCodeAsync();
            await Task.WhenAll(ev, cvc);

            // Capture on the (threadpool) continuation, then mutate the
            // ObservableCollection bindings on the UI thread — MAUI/WinUI
            // silently ignores ItemsSource binding updates raised off the UI
            // thread (the await resumes off-thread because LocalDataService
            // uses ConfigureAwait(false)). Re-notify HasData (which derives
            // from the chart collection counts) after the populate so any
            // IsVisible="{Binding HasData}" gate updates.
            // Mirrors the DashboardViewModel fix.
            var evList = ev.Result;
            var cvcList = cvc.Result;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                EarnedValueTrend.Clear();
                foreach (var p in evList) EarnedValueTrend.Add(p);

                CostVarianceByCode.Clear();
                foreach (var c in cvcList) CostVarianceByCode.Add(c);

                OnPropertyChanged(nameof(HasData));
            });
        });
    }

    // ---- Chart palette (bound from XAML via {Binding …Display} is overkill;
    //      series Fill is set directly in XAML against theme tokens). Nothing
    //      computed here — kept intentionally thin like DashboardViewModel. ----
}
