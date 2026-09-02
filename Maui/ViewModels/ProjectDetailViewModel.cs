using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Construction.Maui.Models;
using Construction.Maui.Services;
using Construction.Maui.ViewModels.Base;

namespace Construction.Maui.ViewModels;

/// <summary>
/// Project Detail ViewModel — mirrors React/Blazor ProjectDetailPage. Tabs:
/// Overview (KPIs + Risks + PDF preview) · Schedule (tasks grid) · Cost
/// (budget + change orders) · RFIs · Submittals. Loads in parallel after the
/// project id is set via LoadAsync(id).
/// </summary>
public sealed partial class ProjectDetailViewModel : ViewModelBase
{
    private readonly IProjectsService _projects;
    private int _projectId;

    public ProjectDetailViewModel(IProjectsService projects) => _projects = projects;

    [ObservableProperty] private ProjectDto? _project;
    [ObservableProperty] private ProjectKpisDto? _kpis;
    [ObservableProperty] private string _activeTab = "Overview";
    [ObservableProperty] private Stream? _pdfDocumentStream;

    public ObservableCollection<TaskDto> Tasks { get; } = new();
    public ObservableCollection<RiskDto> TopRisks { get; } = new();
    public ObservableCollection<MilestoneDto> UpcomingMilestones { get; } = new();
    public ObservableCollection<RecentDocumentDto> RecentDocuments { get; } = new();
    public ObservableCollection<RfiSummaryDto> Rfis { get; } = new();
    public ObservableCollection<SubmittalSummaryDto> Submittals { get; } = new();
    public ObservableCollection<ChangeOrderSummaryDto> ChangeOrders { get; } = new();

    public IReadOnlyList<string> Tabs { get; } = new[] { "Overview", "Schedule", "Cost", "RFIs", "Submittals" };

    public override bool HasData => Project is not null;

    public int? TabBadgeRfis       => Kpis?.OpenRfis;
    public int? TabBadgeSubmittals => Kpis?.OpenSubmittals;
    public int? TabBadgeRisks      => Kpis?.OpenRisks;


    /// <summary>Downloads the sample PDF into a Stream for SfPdfViewer binding.</summary>
    private async Task LoadPdfStreamAsync()
    {
        try
        {
            using var http = new HttpClient();
            var bytes = await http.GetByteArrayAsync(MauiProgram.SamplePdfUrl);
            PdfDocumentStream?.Dispose();
            PdfDocumentStream = new MemoryStream(bytes);
        }
        catch
        {
            // PDF preview is non-fatal — leave PdfDocumentStream null and the viewer hidden.
            PdfDocumentStream = null;
        }
    }
    public string PdfViewerSource => MauiProgram.SamplePdfUrl; // same sample doc used by React/Blazor ports

    public override async Task LoadAsync()
    {
        // Called by AppShellLayout when the page becomes active — only runs when
        // the id has been set (see LoadAsync(int) below).
        if (_projectId <= 0) return;
        await LoadCoreAsync();
    }

    /// <summary>Called before navigation with the route parameter (project id).</summary>
    public async Task LoadAsync(int projectId)
    {
        _projectId = projectId;
        await LoadCoreAsync();
    }

    private async Task LoadCoreAsync()
    {
        await RunSafeAsync(async () =>
        {
            var p    = _projects.GetProjectAsync(_projectId);
            var k    = _projects.GetProjectKpisAsync(_projectId);
            var t    = _projects.GetProjectTasksAsync(_projectId, new QueryParameters { Page = 1, PageSize = 200 });
            var r    = _projects.GetTopOpenRisksAsync(_projectId, 5);

            _ = LoadPdfStreamAsync();
            var m    = _projects.GetUpcomingMilestonesAsync(_projectId, 30);
            var docs = _projects.GetRecentDocumentsAsync(_projectId, 30, 10);
            var rfis = _projects.GetProjectRfisAsync(_projectId);
            var subs = _projects.GetProjectSubmittalsAsync(_projectId);
            var cos  = _projects.GetProjectChangeOrdersAsync(_projectId);
            await Task.WhenAll(p, k, t, r, m, docs, rfis, subs, cos);

            // Capture on the (threadpool) continuation, then mutate the
            // ObservableProperty + ObservableCollection bindings on the UI
            // thread — MAUI/WinUI silently ignores IsVisible/ItemsSource
            // binding updates raised off the UI thread (the await above
            // resumes off-thread because LocalDataService uses
            // ConfigureAwait(false)). Mirrors the DashboardViewModel fix.
            var project  = p.Result;
            var kpis     = k.Result;
            var tasks    = t.Result;
            var risks    = r.Result;
            var miles    = m.Result;
            var docsList = docs.Result;
            var rfisList = rfis.Result;
            var subsList = subs.Result;
            var cosList  = cos.Result;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Project = project;
                Kpis    = kpis;

                Tasks.Clear();              foreach (var x in tasks)    Tasks.Add(x);
                TopRisks.Clear();           foreach (var x in risks)    TopRisks.Add(x);
                UpcomingMilestones.Clear(); foreach (var x in miles)    UpcomingMilestones.Add(x);
                RecentDocuments.Clear();    foreach (var x in docsList) RecentDocuments.Add(x);
                Rfis.Clear();               foreach (var x in rfisList) Rfis.Add(x);
                Submittals.Clear();         foreach (var x in subsList) Submittals.Add(x);
                ChangeOrders.Clear();       foreach (var x in cosList)  ChangeOrders.Add(x);
            });
        });
    }

    // HasData depends on Project; re-notify so IsVisible="{Binding HasData}"
    // sections (Overview KPIs, tabs) update when the source lands.
    partial void OnProjectChanged(ProjectDto? value)
    {
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(ProjectSubtitle));
        OnPropertyChanged(nameof(DurationDisplay));
    }

    // Re-raise every React-parity KPI/trend display prop when Kpis lands so the
    // Overview + Cost tabs update from their collapsed initial state. HasData also
    // re-notifies here even though it officially depends on Project, so a late
    // Kpis arrival after Project also flips the IsVisible gates.
    partial void OnKpisChanged(ProjectKpisDto? value)
    {
        OnPropertyChanged(nameof(HasData));
        OnPropertyChanged(nameof(TabBadgeRfis));
        OnPropertyChanged(nameof(TabBadgeSubmittals));
        OnPropertyChanged(nameof(TabBadgeRisks));
        OnPropertyChanged(nameof(PercentCompleteDisplay));
        OnPropertyChanged(nameof(CostVarianceDisplay));
        OnPropertyChanged(nameof(ScheduleVarianceDisplay));
        OnPropertyChanged(nameof(BudgetDisplay));
        OnPropertyChanged(nameof(SpentDisplay));
        OnPropertyChanged(nameof(CostVarianceIsPositive));
        OnPropertyChanged(nameof(ScheduleVarianceIsPositive));
        OnPropertyChanged(nameof(CostVarianceTrend));
        OnPropertyChanged(nameof(ScheduleVarianceTrend));
        OnPropertyChanged(nameof(RfisHasOverdue));
        OnPropertyChanged(nameof(RfisTrend));
        OnPropertyChanged(nameof(SubmittalsHasOverdue));
        OnPropertyChanged(nameof(SubmittalsTrend));
        OnPropertyChanged(nameof(PendingChangeOrdersAmountDisplay));
        OnPropertyChanged(nameof(PendingChangeOrdersTrend));
        OnPropertyChanged(nameof(PendingChangeOrdersIsWarning));
        OnPropertyChanged(nameof(SpentPercent));
        OnPropertyChanged(nameof(SpentOverBudget));
    }

    // ---- Overview KPI display helpers ----
    public string PercentCompleteDisplay => Kpis is null ? "—" : $"{Kpis.PercentComplete}%";
    public string CostVarianceDisplay    => Kpis is null ? "—" : $"{(Kpis.CostVariance >= 0 ? "+" : "-")}{Math.Abs(Kpis.CostVariance):C0}";
    public string ScheduleVarianceDisplay => Kpis is null ? "—" : $"{(Kpis.ScheduleVariance >= 0 ? "+" : "")}{Kpis.ScheduleVariance:0.0}%";
    public string BudgetDisplay          => Kpis is null ? "—" : Kpis.Budget.ToString("C0");
    public string SpentDisplay           => Kpis is null ? "—" : Kpis.Spent.ToString("C0");

    // ---- Overview KPI React-parity helpers ----
    // Tone booleans drive the trend label color via AppThemeBinding in XAML.
    public bool CostVarianceIsPositive  => Kpis is null || Kpis.CostVariance >= 0;
    public bool ScheduleVarianceIsPositive => Kpis is null || Kpis.ScheduleVariance >= 0;
    public string CostVarianceTrend       => Kpis is null ? "—" : (Kpis.CostVariance >= 0 ? "Under budget" : "Over budget");
    public string ScheduleVarianceTrend   => Kpis is null ? "—" : (Kpis.ScheduleVariance >= 0 ? "Ahead of schedule" : "Behind schedule");
    // RFI overdue tone (warning if overdue>0 else positive)
    public bool RfisHasOverdue             => Kpis is { OverdueRfis: > 0 };
    public string RfisTrend                => Kpis is null ? "—" : (Kpis.OverdueRfis > 0 ? $"{Kpis.OverdueRfis} overdue" : "No overdue");
    // Submittals overdue tone
    public bool SubmittalsHasOverdue       => Kpis is { OverdueSubmittals: > 0 };
    public string SubmittalsTrend          => Kpis is null ? "—" : (Kpis.OverdueSubmittals > 0 ? $"{Kpis.OverdueSubmittals} overdue" : "On track");
    // Pending Change Orders KPI (React shows the $ amount + "N open" caption)
    public string PendingChangeOrdersAmountDisplay => Kpis is null ? "—" : Kpis.PendingChangeOrdersAmount.ToString("C0");
    public string PendingChangeOrdersTrend => Kpis is null ? "—" : (Kpis.PendingChangeOrdersAmount > 0 ? $"{Kpis.OpenChangeOrders} pending review" : "No pending COs");
    public bool PendingChangeOrdersIsWarning => Kpis is { PendingChangeOrdersAmount: > 0 };

    // ---- Header subtitle (React parity: location · dateRange · budget) ----
    public string ProjectSubtitle
    {
        get
        {
            if (Project is null) return string.Empty;
            var parts = new List<string>(3);
            if (!string.IsNullOrWhiteSpace(Project.Location)) parts.Add(Project.Location);
            if (Project.StartDate != default && Project.EndDate != default)
                parts.Add($"{Project.StartDate:MMM d, yyyy} – {Project.EndDate:MMM d, yyyy}");
            if (Project.Budget != 0) parts.Add($"{Project.Budget:C0} budget");
            return string.Join(" · ", parts);
        }
    }

    // Meta-list "Duration" value (React parity: startDate – endDate).
    public string DurationDisplay => Project is null
        ? "—"
        : $"{Project.StartDate:MMM d, yyyy} – {Project.EndDate:MMM d, yyyy}";

    // ---- Cost tab React-parity helpers ----
    // Spent progress (red if >90% of budget, else success) — reacts to Kpis.
    public double SpentPercent => (Kpis is null || Kpis.Budget <= 0) ? 0 : Math.Min(100.0, (double)Math.Round(Kpis.Spent / Kpis.Budget * 100, 0));
    public bool SpentOverBudget => SpentPercent > 90;
}
