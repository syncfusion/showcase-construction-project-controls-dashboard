using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Construction.Maui.Models;
using Construction.Maui.Services;
using Construction.Maui.ViewModels.Base;

namespace Construction.Maui.ViewModels;

/// <summary>
/// Projects page ViewModel — mirrors React/Blazor ProjectsPage data binding:
/// server-side paged project list, status filter + search + sort, row click
/// navigates to Project Detail.
/// </summary>
public sealed partial class ProjectsViewModel : ViewModelBase
{
    private readonly IProjectsService _projects;

    public ProjectsViewModel(IProjectsService projects) => _projects = projects;

    public ObservableCollection<ProjectDto> Projects { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string? _selectedStatus; // null = All
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _totalPages;

    public IReadOnlyList<string> StatusOptions { get; } = new[] { "All", "Active", "Planning", "OnHold", "Completed", "Cancelled" };

    public override bool HasData => Projects.Count > 0;

    public override async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var query = new QueryParameters
            {
                Page = CurrentPage,
                PageSize = 50,
                Search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                Status = SelectedStatus is null or "All" ? null : SelectedStatus,
            };
            var result = await _projects.GetProjectsAsync(query);

            // Capture on the (threadpool) continuation, then mutate the
            // ObservableCollection + ObservableProperty bindings on the UI
            // thread — MAUI/WinUI silently ignores ItemsSource/IsVisible
            // binding updates raised off the UI thread (the await resumes
            // off-thread because LocalDataService uses ConfigureAwait(false)).
            // Re-notify HasData (which derives from Projects.Count) after the
            // populate so any IsVisible="{Binding HasData}" gate updates.
            // Mirrors the DashboardViewModel fix.
            var data = result.Data;
            var totalCount = result.TotalCount;
            var totalPages = result.TotalPages;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Projects.Clear();
                foreach (var p in data) Projects.Add(p);
                TotalCount = totalCount;
                TotalPages = totalPages;
                OnPropertyChanged(nameof(HasData));
            });
        });
    }

    /// <summary>Called by the code-behind when a row is tapped.</summary>
    public Func<int, Task>? OpenProjectDetail { get; set; }

    [RelayCommand]
    private async Task OpenProject(int id)
    {
        if (OpenProjectDetail is not null) await OpenProjectDetail(id);
    }

    [RelayCommand]
    private async Task Search()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ChangeStatus(string? status)
    {
        SelectedStatus = status;
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextPage()
    {
        if (CurrentPage < TotalPages) { CurrentPage++; await LoadAsync(); }
    }

    [RelayCommand]
    private async Task PreviousPage()
    {
        if (CurrentPage > 1) { CurrentPage--; await LoadAsync(); }
    }

    // ---- Cell formatting helpers (consumed by XAML converters / bindings) ----

    public static string StatusDisplay(ProjectStatus s) => s switch
    {
        ProjectStatus.Planning   => "Planning",
        ProjectStatus.Active     => "Active",
        ProjectStatus.OnHold     => "On Hold",
        ProjectStatus.Completed  => "Completed",
        ProjectStatus.Cancelled  => "Cancelled",
        _ => s.ToString(),
    };

    public static string BudgetDisplay(decimal b) => b.ToString("C0");
    public static string DateDisplay(DateTime d) => d.ToString("yyyy-MM-dd");
}
