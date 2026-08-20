using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;

namespace Construction.Blazor.Components.Pages;

public partial class ProjectDetail : ComponentBase
{
    [Parameter] public int Id { get; set; }

    [Inject] private ProjectsService ProjectsApi { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    // Demo documents don't have real per-file content in the sample data set, so every
    // preview opens the same sample PDF (mirrors the approach used on the Documents page).
    private const string SamplePdfUrl = "https://cdn.syncfusion.com/content/pdf/pdf-succinctly.pdf";

    private static readonly string[] Tabs = ["Overview", "Schedule", "Cost", "RFIs", "Submittals"];
    private string _activeTab = "Overview";

    private static readonly Dictionary<HealthStatus, string> HealthBadgeClass = new()
    {
        [HealthStatus.NotStarted] = "badge-neutral",
        [HealthStatus.OnTrack] = "badge-success",
        [HealthStatus.AtRisk] = "badge-warning",
        [HealthStatus.Critical] = "badge-error",
    };

    private static readonly Dictionary<string, string> RiskAlertClass = new()
    {
        ["Critical"] = "alert-error",
        ["High"] = "alert-warning",
        ["Medium"] = "alert-info",
        ["Low"] = "alert-info",
    };

    private static readonly Dictionary<TaskStatus, string> MilestoneStatusBadgeClass = new()
    {
        [TaskStatus.NotStarted] = "badge-neutral",
        [TaskStatus.InProgress] = "badge-info",
        [TaskStatus.OnHold] = "badge-warning",
        [TaskStatus.Completed] = "badge-success",
        [TaskStatus.Cancelled] = "badge-error",
    };

    private static readonly Dictionary<TaskStatus, string> MilestoneStatusLabel = new()
    {
        [TaskStatus.NotStarted] = "Not Started",
        [TaskStatus.InProgress] = "In Progress",
        [TaskStatus.OnHold] = "On Hold",
        [TaskStatus.Completed] = "Completed",
        [TaskStatus.Cancelled] = "Cancelled",
    };

    private ProjectDto? _project;
    private ProjectKpisDto? _kpis;
    private List<MilestoneDto> _milestones = [];
    private List<RiskDto> _risks = [];
    private List<RecentDocumentDto> _documents = [];
    private List<RfiSummaryDto> _rfis = [];
    private List<SubmittalSummaryDto> _submittals = [];
    private List<ChangeOrderSummaryDto> _changeOrders = [];
    private bool _loading = true;
    private string? _error;
    private RecentDocumentDto? _previewDocument;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _error = null;
        try
        {
            var projectTask = ProjectsApi.GetByIdAsync(Id);
            var kpisTask = ProjectsApi.GetKpisAsync(Id);
            var milestonesTask = ProjectsApi.GetUpcomingMilestonesAsync(Id, 30, 10);
            var risksTask = ProjectsApi.GetTopRisksAsync(Id, 5);
            var documentsTask = ProjectsApi.GetRecentDocumentsAsync(Id, 30, 10);
            var rfisTask = ProjectsApi.GetRfisAsync(Id, 50);
            var submittalsTask = ProjectsApi.GetSubmittalsAsync(Id, 50);
            var changeOrdersTask = ProjectsApi.GetChangeOrdersAsync(Id, 50);
            await Task.WhenAll(projectTask, kpisTask, milestonesTask, risksTask, documentsTask, rfisTask, submittalsTask, changeOrdersTask);

            _project = projectTask.Result;
            _kpis = kpisTask.Result;
            _milestones = milestonesTask.Result;
            _risks = risksTask.Result;
            _documents = documentsTask.Result;
            _rfis = rfisTask.Result;
            _submittals = submittalsTask.Result;
            _changeOrders = changeOrdersTask.Result;
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

    private string HeaderSubtitle
    {
        get
        {
            if (_project is null) return "";
            var parts = new List<string?>
            {
                _project.Location,
                $"{Formatters.FormatDate(_project.StartDate)} – {Formatters.FormatDate(_project.EndDate)}",
                $"{Formatters.FormatCurrency(_project.Budget)} budget",
            }.Where(p => !string.IsNullOrEmpty(p));
            return string.Join(" · ", parts);
        }
    }

    private int TabBadge(string tab) => tab switch
    {
        "RFIs" => _kpis?.OpenRfis ?? 0,
        "Submittals" => _kpis?.OpenSubmittals ?? 0,
        _ => 0,
    };

    private static string SplitHealthLabel(HealthStatus status) =>
        System.Text.RegularExpressions.Regex.Replace(status.ToString(), "([A-Z])", " $1").Trim();

    private static string FormatPercent(decimal n) => $"{(n > 0 ? "+" : "")}{n}%";

    private void GoBack() => Nav.NavigateTo("/projects");
    private void GoTo(string path) => Nav.NavigateTo(path);

    private void OnOpenRfisKeydown(KeyboardEventArgs e) => KeyboardActivation.OnActivateKey(e, () => GoTo("/rfis"));

    private static string DocumentStatusClass(string status)
    {
        var mapped = status.ToLowerInvariant();
        if (mapped is "approved" or "answered" or "uploaded") return "badge-success";
        if (mapped is "under review" or "submitted" or "draft") return "badge-warning";
        if (mapped is "rejected") return "badge-error";
        return "badge-info";
    }

    private static string ChangeOrderStatusClass(ChangeOrderStatus status) => status switch
    {
        ChangeOrderStatus.Approved or ChangeOrderStatus.Implemented => "badge-success",
        ChangeOrderStatus.Pending => "badge-warning",
        ChangeOrderStatus.Rejected => "badge-error",
        _ => "badge-info",
    };

    private static string RfiStatusClass(RFIStatus status) => status switch
    {
        RFIStatus.Answered or RFIStatus.Closed => "badge-success",
        RFIStatus.Open or RFIStatus.UnderReview or RFIStatus.InReview or RFIStatus.Submitted => "badge-warning",
        _ => "badge-info",
    };

    private static string SubmittalStatusClass(SubmittalStatus status) => status switch
    {
        SubmittalStatus.Approved or SubmittalStatus.ApprovedWithComments => "badge-success",
        SubmittalStatus.Draft or SubmittalStatus.Submitted or SubmittalStatus.UnderReview => "badge-warning",
        SubmittalStatus.Rejected => "badge-error",
        _ => "badge-info",
    };

    private static int SpentPercent(decimal spent, decimal budget) =>
        budget == 0 ? 0 : Math.Min(100, (int)Math.Round(spent / budget * 100));

    // Syncfusion grid row select — opens the document preview modal.
    private void OnDocumentRowSelected(RowSelectEventArgs<RecentDocumentDto> args)
    {
        if (args.Data is not null) _previewDocument = args.Data;
    }
}
