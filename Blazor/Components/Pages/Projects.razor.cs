using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Grids;

namespace Construction.Blazor.Components.Pages;

public partial class Projects : ComponentBase
{
    [Inject] private ProjectsService ProjectsApi { get; set; } = default!;
    [Inject] private DownloadInterop Download { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private SfGrid<ProjectDto>? _grid;

    private static readonly (ProjectStatus? Status, string Label)[] StatusOptions =
    [
        (null, "All statuses"), (ProjectStatus.Active, "Active"), (ProjectStatus.Planning, "Planning"),
        (ProjectStatus.OnHold, "OnHold"), (ProjectStatus.Completed, "Completed"), (ProjectStatus.Cancelled, "Cancelled"),
    ];

    private static readonly ProjectStatus[] NewProjectStatusOptions =
        [ProjectStatus.Planning, ProjectStatus.Active, ProjectStatus.OnHold, ProjectStatus.Completed, ProjectStatus.Cancelled];

    private static readonly Dictionary<ProjectStatus, string> StatusBadgeClass = new()
    {
        [ProjectStatus.Active] = "badge-success",
        [ProjectStatus.Planning] = "badge-info",
        [ProjectStatus.OnHold] = "badge-warning",
        [ProjectStatus.Completed] = "badge-info",
        [ProjectStatus.Cancelled] = "badge-neutral",
    };

    private List<ProjectDto> _projects = [];
    private ProjectStatus? _status;
    private string _search = "";
    private bool _loading = true;
    private string? _error;

    private bool _showNewProjectModal;
    private ProjectCreateDto _draft = NewDraft();

    private static ProjectCreateDto NewDraft() => new()
    {
        StartDate = DateTime.Today,
        EndDate = DateTime.Today,
        Status = ProjectStatus.Planning,
    };

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        _error = null;
        try
        {
            var result = await ProjectsApi.GetProjectsAsync(1, 1000);
            _projects = result.Data.ToList();
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

    private List<ProjectDto> FilteredProjects =>
        _projects.Where(p =>
        {
            var matchesStatus = _status is null || p.Status == _status;
            var q = _search.Trim();
            var matchesSearch = string.IsNullOrEmpty(q)
                || p.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || p.Code.Contains(q, StringComparison.OrdinalIgnoreCase)
                || (p.Location ?? "").Contains(q, StringComparison.OrdinalIgnoreCase);
            return matchesStatus && matchesSearch;
        }).ToList();

    private void OnSearchChanged(ChangeEventArgs e) => _search = e.Value?.ToString() ?? "";

    private void OnStatusChanged(ChangeEventArgs e)
    {
        var value = e.Value?.ToString();
        _status = string.IsNullOrEmpty(value) ? null : Enum.Parse<ProjectStatus>(value);
    }

    private void GoToProject(int id) => Nav.NavigateTo($"/projects/{id}");

    // Syncfusion grid row select — opens the project detail page.
    private void OnProjectRowSelected(RowSelectEventArgs<ProjectDto> args)
    {
        if (args.Data is not null) GoToProject(args.Data.Id);
    }

    private void OpenNewProjectModal()
    {
        _draft = NewDraft();
        _showNewProjectModal = true;
    }

    private void SaveNewProject()
    {
        if (string.IsNullOrWhiteSpace(_draft.Name)) return;
        var nextId = _projects.Count > 0 ? _projects.Max(p => p.Id) + 1 : 1;
        var created = new ProjectDto
        {
            Id = nextId,
            Name = _draft.Name.Trim(),
            Code = string.IsNullOrWhiteSpace(_draft.Code) ? $"PRJ-{nextId:D4}" : _draft.Code.Trim(),
            Description = string.IsNullOrWhiteSpace(_draft.Description) ? null : _draft.Description.Trim(),
            StartDate = _draft.StartDate,
            EndDate = _draft.EndDate,
            Status = _draft.Status,
            Location = string.IsNullOrWhiteSpace(_draft.Location) ? null : _draft.Location.Trim(),
            Budget = _draft.Budget,
            Progress = 0,
            Manager = string.IsNullOrWhiteSpace(_draft.Manager) ? null : _draft.Manager.Trim(),
            CreatedDate = DateTime.UtcNow,
            HealthStatus = HealthStatus.NotStarted,
        };
        // Demo only: kept in local component state so it's visible in the UI immediately;
        // nothing is written back to the API.
        _projects = [created, .. _projects];
        _status = null;
        _search = "";
        _showNewProjectModal = false;
    }

    private async Task ExportProjects()
    {
        var csv = CsvBuilder.Build<ProjectDto>(
        [
            new("Project ID", p => p.Code),
            new("Name", p => p.Name),
            new("Location", p => p.Location ?? ""),
            new("Start Date", p => Formatters.FormatDate(p.StartDate)),
            new("Finish Date", p => Formatters.FormatDate(p.EndDate)),
            new("Progress (%)", p => p.Progress),
            new("Budget", p => p.Budget),
            new("Status", p => p.Status),
        ], FilteredProjects);
        await Download.DownloadTextFileAsync("projects.csv", "text/csv;charset=utf-8;", csv);
    }

    private static string ProgressTone(int progress) => progress >= 75 ? "is-success" : progress >= 40 ? "" : "is-warning";
}
