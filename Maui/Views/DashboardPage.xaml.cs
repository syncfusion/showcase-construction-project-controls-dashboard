using Construction.Maui.Controls;
using Construction.Maui.Core;
using Construction.Maui.ViewModels;

namespace Construction.Maui.Views;

public partial class DashboardPage : ScreenBase
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        // Adaptive chart-grid reflow (Phase 3). Hooks the page's own SizeChanged so
        // ChartsGrid re-columns as the window / shell mode changes. The KPI cards
        // no longer need Reflow — they're in a FlexLayout (KpiGrid) that auto-wraps
        // 3-per-row → 2 → 1 based on its own children's Basis+Grow, mirroring
        // React's `.kpi-grid { grid-template-columns: repeat(auto-fit, minmax(220px,1fr)) }`.
        SizeChanged += (_, _) =>
        {
            AdaptiveGrids.Reflow(ChartsGrid, Width, maxColumns: 2);
        };
    }
}
