using Construction.Maui.Controls;
using Construction.Maui.Core;
using Construction.Maui.ViewModels;

namespace Construction.Maui.Views;

public partial class CostControlPage : ScreenBase
{
    public CostControlPage(CostControlViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Adaptive KPI grid reflow (Phase 3). The original 3-col / 2-row grid
        // collapses to 2 / 1 columns on narrower widths.
        SizeChanged += (_, _) => AdaptiveGrids.Reflow(KpiGrid, Width, maxColumns: 3);
    }
}
