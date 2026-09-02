using Construction.Maui.Controls;
using Construction.Maui.ViewModels;

namespace Construction.Maui.Views;

public partial class RisksPage : ScreenBase
{
    public RisksPage(RisksViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    /// <summary>Picker SelectedItem doesn't raise SearchCommand — trigger reload manually.</summary>
    private async void OnApplyFilters(object? sender, EventArgs e)
    {
        if (BindingContext is RisksViewModel vm) await vm.LoadAsync();
    }
}
