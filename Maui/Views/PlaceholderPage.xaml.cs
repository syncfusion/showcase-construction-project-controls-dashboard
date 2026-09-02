using Construction.Maui.Controls;

namespace Construction.Maui.Views;

/// <summary>
/// Lightweight "Coming soon" placeholder screen for P1/P2 nav items
/// (Site Map, Calendar, Documents, Contractors, Workflows, Reports)
/// displays a styled message without implementing full module logic.
/// </summary>
public partial class PlaceholderPage : ScreenBase
{
    public PlaceholderPage(string title)
    {
        InitializeComponent();
        TitleLabel.Text = title;
    }
}
