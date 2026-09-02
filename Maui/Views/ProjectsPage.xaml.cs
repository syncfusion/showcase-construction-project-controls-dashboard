using Construction.Maui.Controls;
using Construction.Maui.Models;
using Construction.Maui.ViewModels;

namespace Construction.Maui.Views;

/// <summary>
/// Projects screen code-behind. Mirrors React/Blazor: row tap opens the Project
/// Detail page (pushed onto the NavigationPage stack by AppShellLayout).
/// </summary>
public partial class ProjectsPage : ScreenBase
{
    /// <summary>Set by AppShellLayout so the screen can request a detail navigation.</summary>
    public Func<int, Task>? OpenProjectDetail { get; set; }

    public ProjectsPage(ProjectsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private async void OnGridSelectionChanged(object? sender, Syncfusion.Maui.DataGrid.DataGridSelectionChangedEventArgs e)
    {
        if (e.AddedRows is null || e.AddedRows.Count == 0) return;
        if (e.AddedRows[0] is not ProjectDto project) return;

        // Defer one cycle so the tap gesture finishes before we navigate.
        await Task.Delay(50);

        // Allow the user to re-select the same row later.
        if (sender is Syncfusion.Maui.DataGrid.SfDataGrid grid)
        {
            grid.SelectedRows.Clear();
        }

        if (OpenProjectDetail is not null)
        {
            await OpenProjectDetail(project.Id);
        }
    }
}
