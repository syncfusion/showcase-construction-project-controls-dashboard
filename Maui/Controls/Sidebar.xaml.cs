using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Construction.Maui.Controls;

/// <summary>A single sidebar navigation entry. <see cref="IsSelected"/> is
/// toggled by <see cref="Sidebar.SetActive"/> so the row's highlight is
/// independent of the CollectionView's SelectedItem lifecycle (which the
/// OnSelectionChanged handler resets to null to allow re-clicking the same
/// item — that reset was clearing the Selected visual state and leaving the
/// active row un-highlighted).</summary>
public sealed class NavItem : INotifyPropertyChanged
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public FontImageSource Icon { get; init; } = default!;
    public bool IsImplemented { get; init; } = true;

    private bool _isSelected;
    /// <summary>True when this item is the active route. Bound to the row's
    /// background so the highlight persists even though the CollectionView's
    /// SelectedItem is reset to null after each tap.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectionBackground));
            OnPropertyChanged(nameof(SelectionTextColor));
        }
    }

    /// <summary>Row background for the active route (brand tint) vs transparent.
    /// Hex values mirror Colors.xaml: Brand50 (#EAF1FE) light, Gray800 (#18212F) dark.</summary>
    public Color SelectionBackground =>
        _isSelected
            ? Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromRgb(0x18, 0x21, 0x2F)   // Gray800
                : Color.FromRgb(0xEA, 0xF1, 0xFE)   // Brand50
            : Colors.Transparent;

    /// <summary>Icon/label color for the active route (brand) vs neutral.
    /// Hex values mirror Colors.xaml: Brand700 (#1D4EC5) light active, Brand200
    /// (#AEC8FC) dark active, Gray900 (#101828) light inactive, Gray100 (#F2F4F7) dark inactive.</summary>
    public Color SelectionTextColor =>
        _isSelected
            ? Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromRgb(0xAE, 0xC8, 0xFC)   // Brand200
                : Color.FromRgb(0x1D, 0x4E, 0xC5)   // Brand700
            : Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromRgb(0xF2, 0xF4, 0xF7)   // Gray100
                : Color.FromRgb(0x10, 0x18, 0x28); // Gray900

    public event PropertyChangedEventHandler? PropertyChanged;
    internal void RaisePropertyChanged(string propertyName) => OnPropertyChanged(propertyName);
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class Sidebar : ContentView
{
    public static readonly BindableProperty PrimaryItemsProperty =
        BindableProperty.Create(nameof(PrimaryItems), typeof(ObservableCollection<NavItem>), typeof(Sidebar), defaultValueCreator: _ => new ObservableCollection<NavItem>());

    public static readonly BindableProperty SecondaryItemsProperty =
        BindableProperty.Create(nameof(SecondaryItems), typeof(ObservableCollection<NavItem>), typeof(Sidebar), defaultValueCreator: _ => new ObservableCollection<NavItem>());

    public static readonly BindableProperty NavigateCommandProperty =
        BindableProperty.Create(nameof(NavigateCommand), typeof(ICommand), typeof(Sidebar));

    public ObservableCollection<NavItem> PrimaryItems => (ObservableCollection<NavItem>)GetValue(PrimaryItemsProperty);
    public ObservableCollection<NavItem> SecondaryItems => (ObservableCollection<NavItem>)GetValue(SecondaryItemsProperty);
    public ICommand NavigateCommand { get => (ICommand)GetValue(NavigateCommandProperty); set => SetValue(NavigateCommandProperty, value); }

    public Sidebar()
    {
        // IMPORTANT: assign per-instance collections BEFORE InitializeComponent so
        // the XAML {Binding PrimaryItems, Source={x:Reference Root}} binds resolve
        // to this instance's own list. Without this, BindableProperty's default
        // (`new ObservableCollection<NavItem>()`) is a SHARED singleton across all
        // Sidebar instances — the persistent sidebar AND the drawer would both
        // mutate the SAME list, so Populate() added each item twice → "all pages
        // listed twice" in the sidebar.
        SetValue(PrimaryItemsProperty, new ObservableCollection<NavItem>());
        SetValue(SecondaryItemsProperty, new ObservableCollection<NavItem>());

        InitializeComponent();
        BindingContext = this;
        Populate();
    }

    /// <summary>Builds the default nav set — only the 4 primary pages
    /// (Dashboard, Projects, Cost Control, Risks & Issues). The Management
    /// secondary section was removed; see Sidebar.xaml.</summary>
    private void Populate()
    {
        PrimaryItems.Add(new NavItem { Key = "dashboard",    Label = "Dashboard",     Icon = Icon.Dashboard });
        PrimaryItems.Add(new NavItem { Key = "projects",     Label = "Projects",      Icon = Icon.Projects });
        PrimaryItems.Add(new NavItem { Key = "cost-control", Label = "Cost Control",  Icon = Icon.CostControl });
        PrimaryItems.Add(new NavItem { Key = "risks",        Label = "Risks & Issues", Icon = Icon.Risks });
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        var item = (NavItem)e.CurrentSelection[0]!;
        if (sender is CollectionView cv) cv.SelectedItem = null; // allow re-clicking same item
        if (NavigateCommand?.CanExecute(item) == true)
            NavigateCommand.Execute(item);
    }

    /// <summary>Highlights the active nav item by key. Sets <see cref="NavItem.IsSelected"/>
    /// on the matching item and clears it on the rest so the row's background
    /// binding updates reliably (independent of the CollectionView's SelectedItem,
    /// which OnSelectionChanged resets to null).</summary>
    public void SetActive(string key)
    {
        foreach (var item in PrimaryItems)
            item.IsSelected = string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase);
        foreach (var item in SecondaryItems)
            item.IsSelected = string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase);
        // Keep the CollectionView's SelectedItem aligned for accessibility/AT,
        // but the visual highlight is driven by NavItem.IsSelected above so it
        // survives the OnSelectionChanged null-reset.
        var primary = PrimaryItems.FirstOrDefault(x => x.Key == key);
        if (primary is not null && !ReferenceEquals(PrimaryNav.SelectedItem, primary))
            PrimaryNav.SelectedItem = primary;
    }

    /// <summary>Re-evaluates theme-derived colors on every item (called by
    /// AppShellLayout after a light/dark toggle so the active-row tint swaps).</summary>
    public void RefreshThemeColors()
    {
        foreach (var item in PrimaryItems.Concat(SecondaryItems))
        {
            if (item.IsSelected)
            {
                item.RaisePropertyChanged(nameof(NavItem.SelectionBackground));
                item.RaisePropertyChanged(nameof(NavItem.SelectionTextColor));
            }
        }
    }

    /// <summary>Adaptive shell mode driven by <see cref="AppShellLayout"/>'s
    /// width/idiom logic. Maps to the <c>SidebarMode</c> visual-state group in
    /// Sidebar.xaml so the rail collapses to icons-only without losing layout.
    /// Also collapses the brand text + footer caption in Rail mode (those can't
    /// be reached from a XAML VSM on the root Grid, so they're toggled here).</summary>
    public void SetMode(SidebarMode mode)
    {
        _mode = mode;
        VisualStateManager.GoToState(this, mode.ToString());

        var showText = mode == SidebarMode.Full;
        BrandLabel.IsVisible = showText;
        FooterLabel.IsVisible = showText;
    }

    private SidebarMode _mode = SidebarMode.Full;
}

/// <summary>Shell modes the sidebar can render in.</summary>
public enum SidebarMode { Full, Rail, Hidden }
