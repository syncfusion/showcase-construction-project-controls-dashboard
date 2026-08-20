using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;

namespace Construction.Blazor.Components.Pages;

public partial class Risks : ComponentBase
{
    [Inject] private RisksService RisksApi { get; set; } = default!;
    [Inject] private RiskMatrixService RiskMatrixApi { get; set; } = default!;
    [Inject] private DownloadInterop Download { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private SfGrid<RiskDto>? _risksGrid;

    private static readonly Dictionary<RiskSeverity, string> SeverityBadgeClass = new()
    {
        [RiskSeverity.Critical] = "badge-error", [RiskSeverity.High] = "badge-warning",
        [RiskSeverity.Medium] = "badge-info", [RiskSeverity.Low] = "badge-neutral",
    };

    private static readonly Dictionary<RiskStatus, string> StatusBadgeClass = new()
    {
        [RiskStatus.Open] = "badge-error", [RiskStatus.InProgress] = "badge-warning", [RiskStatus.Monitoring] = "badge-info",
        [RiskStatus.Escalated] = "badge-error", [RiskStatus.Containment] = "badge-warning",
        [RiskStatus.Mitigated] = "badge-success", [RiskStatus.Closed] = "badge-neutral",
    };

    private enum KpiKey { Critical, High, Medium, Mitigated }

    private static readonly Dictionary<KpiKey, string> KpiBorderColor = new()
    {
        [KpiKey.Critical] = "var(--color-error)", [KpiKey.High] = "var(--color-warning)",
        [KpiKey.Medium] = "var(--color-warning)", [KpiKey.Mitigated] = "var(--color-success)",
    };

    private static readonly Dictionary<KpiKey, string> KpiIcon = new()
    {
        [KpiKey.Critical] = "shield-alert", [KpiKey.High] = "alert-triangle", [KpiKey.Medium] = "activity", [KpiKey.Mitigated] = "check-circle",
    };

    private static readonly Dictionary<KpiKey, string> KpiChangeTone = new()
    {
        [KpiKey.Critical] = "negative", [KpiKey.High] = "warning", [KpiKey.Medium] = "text-secondary", [KpiKey.Mitigated] = "positive",
    };

    private static readonly Dictionary<KpiKey, string> KpiChangeLabel = new()
    {
        [KpiKey.Critical] = "Immediate action required", [KpiKey.High] = "Watch closely",
        [KpiKey.Medium] = "Monitored", [KpiKey.Mitigated] = "On track",
    };

    private static readonly (RiskSeverity? Severity, string Label)[] SeverityOptions =
        [(null, "All"), (RiskSeverity.Critical, "Critical"), (RiskSeverity.High, "High"), (RiskSeverity.Medium, "Medium"), (RiskSeverity.Low, "Low")];

    private static readonly (RiskStatus? Status, string Label)[] StatusOptions =
    [
        (null, "All"), (RiskStatus.Open, "Open"), (RiskStatus.InProgress, "InProgress"), (RiskStatus.Monitoring, "Monitoring"),
        (RiskStatus.Escalated, "Escalated"), (RiskStatus.Containment, "Containment"), (RiskStatus.Mitigated, "Mitigated"), (RiskStatus.Closed, "Closed"),
    ];

    private static readonly RiskSeverity[] Severities = [RiskSeverity.Low, RiskSeverity.Medium, RiskSeverity.High, RiskSeverity.Critical];
    private static readonly RiskProbability[] Probabilities = [RiskProbability.Low, RiskProbability.Medium, RiskProbability.High];
    private static readonly RiskProbability[] ReversedProbabilities = [.. Probabilities.Reverse()];

    private List<RiskDto> _risks = [];
    private RiskKpisDto? _kpis;
    private List<RiskMatrixCellViewModel> _matrix = [];
    private bool _loading = true;
    private string? _error;

    private string _search = "";
    private RiskSeverity? _severity;
    private RiskStatus? _status;

    private RiskDto? _selectedRisk;
    private bool _showNewRiskModal;
    private NewRiskDraft _draft = new();

    private class NewRiskDraft
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ProjectId { get; set; } = "";
        public string ProjectCode { get; set; } = "";
        public RiskSeverity Severity { get; set; } = RiskSeverity.Medium;
        public RiskProbability Probability { get; set; } = RiskProbability.Medium;
        public string Owner { get; set; } = "";
        public string MitigationPlan { get; set; } = "";
        public DateTime? TargetResolutionDate { get; set; }
        public string ImpactDays { get; set; } = "";
        public string ImpactCost { get; set; } = "";
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var risksTask = RisksApi.GetRisksAsync(1, 1000);
            var kpisTask = RisksApi.GetKpisAsync();
            var matrixTask = RiskMatrixApi.GetMatrixAsync();
            await Task.WhenAll(risksTask, kpisTask, matrixTask);
            _risks = risksTask.Result.Data.ToList();
            _kpis = kpisTask.Result;
            _matrix = matrixTask.Result;
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

    private static string FormatImpact(int? impactDays, decimal? impactCost)
    {
        var parts = new List<string>();
        if (impactDays is > 0) parts.Add($"{impactDays}d");
        if (impactCost is > 0) parts.Add($"${(impactCost.Value / 1_000_000m):0.0}M");
        return parts.Count == 0 ? "Minor" : string.Join(" · ", parts);
    }

    private List<RiskDto> FilteredRisks =>
        _risks.Where(r =>
        {
            var q = _search.Trim();
            var matchesSearch = string.IsNullOrEmpty(q)
                || r.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.Number.Contains(q, StringComparison.OrdinalIgnoreCase)
                || r.ProjectCode.Contains(q, StringComparison.OrdinalIgnoreCase);
            var matchesSeverity = _severity is null || r.Severity == _severity;
            var matchesStatus = _status is null || r.Status == _status;
            return matchesSearch && matchesSeverity && matchesStatus;
        }).ToList();

    private (KpiKey Key, string Label, int Value)[]? KpiSummary => _kpis is null ? null :
    [
        (KpiKey.Critical, "Critical", _kpis.Critical),
        (KpiKey.High, "High", _kpis.High),
        (KpiKey.Medium, "Medium", _kpis.Medium),
        (KpiKey.Mitigated, "Mitigated this month", _kpis.MitigatedThisMonth),
    ];

    private RiskMatrixCellViewModel? MatrixCell(RiskProbability probability, RiskSeverity severity) =>
        _matrix.FirstOrDefault(c => c.Probability == probability.ToString() && c.Severity == severity.ToString());

    private string MatrixCellDisplay(RiskProbability probability, RiskSeverity severity)
    {
        var cell = MatrixCell(probability, severity);
        if (cell is null) return "";
        var ids = cell.RiskIds.Take(2).ToList();
        var overflow = cell.Count > ids.Count ? $" +{cell.Count - ids.Count}" : "";
        return string.Join(", ", ids) + overflow;
    }

    private static string MatrixTone(RiskSeverity severity, RiskProbability probability)
    {
        if (severity == RiskSeverity.Critical || (severity == RiskSeverity.High && probability == RiskProbability.High)) return "negative";
        if (severity == RiskSeverity.High || probability == RiskProbability.High || (severity == RiskSeverity.Medium && probability == RiskProbability.Medium)) return "warning";
        if (severity == RiskSeverity.Low) return "positive";
        return "info";
    }

    private static string MatrixClass(string tone) => tone switch
    {
        "negative" => "is-negative is-emphasis",
        "warning" => "is-warning",
        "positive" => "is-positive",
        _ => "is-info",
    };

    private void OnSearchChanged(ChangeEventArgs e) => _search = e.Value?.ToString() ?? "";

    private void OnSeverityChanged(ChangeEventArgs e)
    {
        var v = e.Value?.ToString();
        _severity = string.IsNullOrEmpty(v) ? null : Enum.Parse<RiskSeverity>(v);
    }

    private void OnStatusChanged(ChangeEventArgs e)
    {
        var v = e.Value?.ToString();
        _status = string.IsNullOrEmpty(v) ? null : Enum.Parse<RiskStatus>(v);
    }

    // Syncfusion grid row select — opens the risk detail modal.
    private void OnRiskRowSelected(RowSelectEventArgs<RiskDto> args)
    {
        if (args.Data is not null) _selectedRisk = args.Data;
    }

    private void ApplyKpiFilter(KpiKey key)
    {
        if (key == KpiKey.Mitigated)
        {
            _status = RiskStatus.Mitigated;
            _severity = null;
            return;
        }
        _severity = key switch { KpiKey.Critical => RiskSeverity.Critical, KpiKey.High => RiskSeverity.High, _ => RiskSeverity.Medium };
        _status = null;
    }

    private void OnKpiKeydown(KeyboardEventArgs e, KpiKey key) => KeyboardActivation.OnActivateKey(e, () => ApplyKpiFilter(key));

    private void ViewProjectDetails()
    {
        if (_selectedRisk is null) return;
        var projectId = _selectedRisk.ProjectId;
        _selectedRisk = null;
        Nav.NavigateTo($"/projects/{projectId}");
    }

    private void OpenNewRiskModal()
    {
        _draft = new NewRiskDraft();
        _showNewRiskModal = true;
    }

    private void SaveNewRisk()
    {
        if (string.IsNullOrWhiteSpace(_draft.Title)) return;
        var nextId = _risks.Count > 0 ? _risks.Max(r => r.Id) + 1 : 1;
        var impactDays = int.TryParse(_draft.ImpactDays, out var d) ? d : (int?)null;
        var impactCost = decimal.TryParse(_draft.ImpactCost, out var c) ? c : (decimal?)null;
        var created = new RiskDto
        {
            Id = nextId,
            ProjectId = int.TryParse(_draft.ProjectId, out var pid) ? pid : 0,
            ProjectCode = string.IsNullOrWhiteSpace(_draft.ProjectCode) ? "TBD" : _draft.ProjectCode.Trim(),
            Number = $"RISK-{nextId:D4}",
            Title = _draft.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(_draft.Description) ? null : _draft.Description.Trim(),
            Severity = _draft.Severity,
            Probability = _draft.Probability,
            ImpactType = impactCost is not null ? RiskImpactType.Cost : impactDays is not null ? RiskImpactType.Schedule : RiskImpactType.Quality,
            ImpactCost = impactCost,
            ImpactDays = impactDays,
            Owner = string.IsNullOrWhiteSpace(_draft.Owner) ? null : _draft.Owner.Trim(),
            Status = RiskStatus.Open,
            MitigationPlan = string.IsNullOrWhiteSpace(_draft.MitigationPlan) ? null : _draft.MitigationPlan.Trim(),
            IdentifiedDate = DateTime.UtcNow,
            TargetResolutionDate = _draft.TargetResolutionDate,
        };
        // Demo only: kept in local component state so it's visible in the UI immediately;
        // nothing is written back to the API.
        _risks = [created, .. _risks];
        _severity = null;
        _status = null;
        _search = "";
        _showNewRiskModal = false;
    }

    private async Task ExportRisks()
    {
        var csv = CsvBuilder.Build<RiskDto>(
        [
            new("ID", r => r.Number),
            new("Risk / Issue", r => r.Title),
            new("Project", r => r.ProjectCode),
            new("Severity", r => r.Severity),
            new("Probability", r => r.Probability),
            new("Impact", r => FormatImpact(r.ImpactDays, r.ImpactCost)),
            new("Owner", r => r.Owner ?? ""),
            new("Status", r => r.Status),
        ], FilteredRisks);
        await Download.DownloadTextFileAsync("risks.csv", "text/csv;charset=utf-8;", csv);
    }
}
