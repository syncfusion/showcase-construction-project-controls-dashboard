using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Construction.Maui.ViewModels.Base;

/// <summary>
/// Common base for ViewModels: INotifyPropertyChanged via CommunityToolkit.Mvvm,
/// IsBusy / ErrorMessage state for the three required UI states (loading, error,
/// content) plus a LoadAsync hook the AppShellLayout invokes when the page is
/// swapped in. Pages bind to IsBusy, ErrorMessage, and HasError.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>True while no error has been raised and no data has loaded yet.</summary>
    public bool IsEmpty => !IsBusy && !HasError && !HasData;

    /// <summary>Override to indicate whether meaningful data is already bound.</summary>
    public virtual bool HasData => false;

    partial void OnErrorMessageChanged(string? value)
        => OnPropertyChanged(nameof(HasError));

    partial void OnIsBusyChanged(bool value)
        => OnPropertyChanged(nameof(IsEmpty));

    /// <summary>Called by AppShellLayout when the page becomes active.</summary>
    public abstract Task LoadAsync();

    /// <summary>Wraps a service call in try/finally + IsBusy + ErrorMessage handling.</summary>
    protected async Task RunSafeAsync(Func<Task> work, [CallerMemberName] string op = "")
    {
        if (IsBusy) return;
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            await work();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"{op} failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
