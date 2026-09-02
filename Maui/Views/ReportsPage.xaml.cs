using Construction.Maui.Controls;
using Construction.Maui.ViewModels;

namespace Construction.Maui.Views;

/// <summary>
/// Reports page code-behind. Inherits <see cref="ScreenBase"/> so it plugs into
/// <see cref="AppShellLayout"/>'s content host. The BindingContext is supplied
/// by the screen factory in <c>AppShellLayout.xaml.cs</c>.
/// </summary>
public partial class ReportsPage : ScreenBase
{
    public ReportsPage(ReportsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
