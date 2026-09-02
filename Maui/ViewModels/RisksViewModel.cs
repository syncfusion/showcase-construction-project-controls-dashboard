using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Construction.Maui.Models;
using Construction.Maui.Services;
using Construction.Maui.ViewModels.Base;

namespace Construction.Maui.ViewModels;

/// <summary>
/// Risks &amp; Issues page ViewModel — mirrors React/Blazor RisksPage:
/// server-paged risks list with severity + status filters and search.
/// </summary>
public sealed partial class RisksViewModel : ViewModelBase
{
    private readonly IRisksService _risks;

    public RisksViewModel(IRisksService risks) => _risks = risks;

    public ObservableCollection<RiskDto> Risks { get; } = new();

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string? _selectedSeverity; // null = All
    [ObservableProperty] private string? _selectedStatus;   // null = All
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _totalPages;

    public IReadOnlyList<string> SeverityOptions { get; } = new[] { "All", "Low", "Medium", "High", "Critical" };
    public IReadOnlyList<string> StatusOptions   { get; } = new[] { "All", "Open", "InProgress", "Monitoring", "Escalated", "Containment", "Mitigated", "Closed" };

    public override bool HasData => Risks.Count > 0;

    public override async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var severity = SelectedSeverity is null or "All" ? null : SelectedSeverity;
            var status   = SelectedStatus   is null or "All" ? null : SelectedStatus;
            var filter = (severity is not null && status is not null)
                ? $"severity={severity};status={status}"
                : (severity is not null ? $"severity={severity}" : (status is not null ? $"status={status}" : null));

            var query = new QueryParameters
            {
                Page = CurrentPage,
                PageSize = 50,
                Search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                Filter = filter,
            };
            var result = await _risks.GetRisksAsync(query);

            // Capture on the (threadpool) continuation, then mutate the
            // ObservableCollection + ObservableProperty bindings on the UI
            // thread — MAUI/WinUI silently ignores ItemsSource/IsVisible
            // binding updates raised off the UI thread (the await resumes
            // off-thread because LocalDataService uses ConfigureAwait(false)).
            // Re-notify HasData (which derives from Risks.Count) after the
            // populate so any IsVisible="{Binding HasData}" gate updates.
            // Mirrors the DashboardViewModel fix.
            var data = result.Data;
            var totalCount = result.TotalCount;
            var totalPages = result.TotalPages;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Risks.Clear();
                foreach (var r in data) Risks.Add(r);
                TotalCount = totalCount;
                TotalPages = totalPages;
                OnPropertyChanged(nameof(HasData));
            });
        });
    }

    [RelayCommand]
    private async Task Search()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NextPage() { if (CurrentPage < TotalPages) { CurrentPage++; await LoadAsync(); } }

    [RelayCommand]
    private async Task PreviousPage() { if (CurrentPage > 1) { CurrentPage--; await LoadAsync(); } }
}
