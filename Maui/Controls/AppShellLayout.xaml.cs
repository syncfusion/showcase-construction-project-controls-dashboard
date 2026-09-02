using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Construction.Maui.ViewModels;
namespace Construction.Maui.Controls;

/// <summary>
/// Root desktop layout: sidebar + topbar + content presenter. Swaps screens (ContentView-based)
/// via SetContent. Screens are defined as ScreenBase descendants and loaded with their
/// matching ViewModels. Detail pages are pushed onto a NavigationPage stack.
///
/// Adaptive behavior (Phase 3): the shell switches between three visual states on
/// <see cref="ContentPage.SizeChanged"/>. Mirrors the React app's breakpoints.
/// <list type="bullet">
/// <item>Wide  (effective width &gt;= 1024): full 140px sidebar, no hamburger.</item>
/// <item>Medium (768..1023): 64px icon rail, no hamburger.</item>
/// <item>Narrow (&lt; 768):     sidebar hidden; hamburger in topbar opens an off-canvas drawer + scrim.</item>
/// </list>
/// Phones/tablets are forced to Narrow/Medium regardless of pixel width via
/// <see cref="DeviceInfo.Idiom"/> so the layout matches platform expectations in landscape.
/// </summary>
public partial class AppShellLayout : ContentPage
{
    /// <summary>React parity breakpoints (px).</summary>
    private const double WideBreakpoint = 1024;
    private const double MediumBreakpoint = 768;

    private readonly Dictionary<string, Func<ContentView>> _screenFactory;
    private ContentView? _current;
    private string? _currentKey;

    public AppShellLayout()
    {
        try
        {
            InitializeComponent();
            Title = "Construction Project Controls";

            // Screen routing table. Keys match Sidebar.NavItem.Key.
            _screenFactory = new(StringComparer.OrdinalIgnoreCase)
            {
                ["dashboard"]    = Make(() => App.Services.GetRequiredService<DashboardViewModel>(),     vm => new Views.DashboardPage(vm)),
                ["projects"]     = Make(() => App.Services.GetRequiredService<ProjectsViewModel>(),      vm => new Views.ProjectsPage(vm) { OpenProjectDetail = OpenProjectDetailAsync }),
                ["cost-control"] = Make(() => App.Services.GetRequiredService<CostControlViewModel>(),   vm => new Views.CostControlPage(vm)),
            ["risks"]        = Make(() => App.Services.GetRequiredService<RisksViewModel>(),          vm => new Views.RisksPage(vm)),
            ["reports"]      = Make(() => App.Services.GetRequiredService<ReportsViewModel>(),       vm => new Views.ReportsPage(vm)),
        };

        Sidebar.NavigateCommand = new Command<NavItem>(OnNavigate);
        // The drawer reuses the Sidebar control; both fire OnNavigate, so taps
        // automatically close the drawer in Narrow mode (see CloseDrawer in OnNavigate).

        // Default screen.
        OnNavigate(Sidebar.PrimaryItems[0]);

        // Adaptive shell: switch states whenever the window/page resizes.
        SizeChanged += OnShellSizeChanged;
        }
        catch (Exception ex)
        {
            App.WriteCrashLog("AppShellLayout.ctor", ex);
            throw;
        }
    }

    private static Func<ContentView> Make<TVm>(Func<TVm> getVm, Func<TVm, ContentView> make) where TVm : class
        => () => make(getVm());

    /// <summary>Updates the page title shown in the topbar.</summary>
    public void SetPageTitle(string title) => PageTitle.Text = title;

    private void OnNavigate(NavItem item)
    {
        // Short-circuit re-entrant duplicate navigations to the already-active screen.
        // The Sidebar's CollectionView auto-fires SelectionChanged when its first item
        // is rendered, which re-enters OnNavigate for the same key while the first
        // DashboardPage is still being inflated — building the same XAML page twice
        // in quick succession deadlocks the MAUI source-gen XAML inflator on the UI
        // thread, which was preventing the window from ever appearing.
        if (string.Equals(_currentKey, item.Key, StringComparison.OrdinalIgnoreCase) && _current is not null)
        {
            CloseDrawer();
            return;
        }
        if (!_screenFactory.TryGetValue(item.Key, out var factory))
        {
            // Feature not in scope — route to the styled placeholder.
            SetContent(new Views.PlaceholderPage(item.Label));
            SetPageTitle(item.Label);
            _currentKey = item.Key;
            CloseDrawer();
            return;
        }

        var screen = factory();
        SetContent(screen);
        SetPageTitle(item.Label);
        _currentKey = item.Key;
        Sidebar.SetActive(item.Key);
        Drawer.SetActive(item.Key);
        CloseDrawer();
    }

    private void SetContent(ContentView screen)
    {
        if (_current == screen) return;
        _current = screen;

        // ContentPresenter.Content accepts any View; ContentView is a View.
        ContentArea.Content = screen;

        // Fire the ViewModel's load hook to fetch data. Wrapped in try/catch so a
        // failing/slow/unreachable API can NEVER crash the app (which previously
        // crash-looped via RestartAgent when TaskCanceledException escaped the
        // fire-and-forget). The error is shown in the ErrorMessage UI state
        // instead, matching the React app's error-state behavior.
        if (screen.BindingContext is ViewModels.Base.ViewModelBase vb)
        {
            _ = LoadSafelyAsync(vb);
        }
    }

    private static async Task LoadSafelyAsync(ViewModels.Base.ViewModelBase vb)
    {
        try
        {
            await vb.LoadAsync();
        }
        catch (Exception ex)
        {
            App.WriteCrashLog("AppShellLayout.LoadSafelyAsync", ex);
            try { vb.GetType().GetProperty("ErrorMessage")?.SetValue(vb, $"Failed to load: {ex.Message}"); } catch { }
        }
    }

    private async Task OpenProjectDetailAsync(int projectId)
    {
        var vm = App.Services.GetRequiredService<ProjectDetailViewModel>();
        var page = new Views.ProjectDetailPage(vm);
        await vm.LoadAsync(projectId);
        await Navigation.PushAsync(page);
    }

    private void OnToggleTheme(object? sender, EventArgs e)
    {
        if (Application.Current is App app)
        {
            app.ToggleTheme();
            ThemeToggle.ImageSource = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Icon.Sun
                : Icon.Moon;
            // The active sidebar row's tint is computed from the current theme;
            // re-raise it so the highlight swaps brand/neutral colors immediately.
            Sidebar.RefreshThemeColors();
            Drawer.RefreshThemeColors();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ThemeToggle.ImageSource = Application.Current?.RequestedTheme == AppTheme.Dark ? Icon.Sun : Icon.Moon;
    }

    // ===== Adaptive shell (Phase 3) =====

    private void OnShellSizeChanged(object? sender, EventArgs e) => ApplyShellLayout();

    /// <summary>
    /// Picks the shell visual state (Wide / Medium / Narrow) from the measured width
    /// OR from <see cref="DeviceInfo.Idiom"/> for phone/tablet, sets the matching
    /// <see cref="VisualStateManager"/> state on <see cref="RootGrid"/>, and tells
    /// the sidebar which of its own modes (Full / Rail) to render.
    /// </summary>
    private void ApplyShellLayout()
    {
        var state = ResolveShellState(Width);
        VisualStateManager.GoToState(RootGrid, state.ToString());

        bool isNarrow = state == ShellMode.Narrow;
        bool isMedium = state == ShellMode.Medium;

        // Hamburger is only visible in Narrow mode.
        Hamburger.IsVisible = isNarrow;

        // The persistent sidebar collapses to a 64px icon rail in Medium mode; in Narrow mode
        // it is fully hidden (ColumnDefinitions=0,*) and the drawer takes over.
        Sidebar.SetMode(isMedium
            ? SidebarMode.Rail
            : (isNarrow ? SidebarMode.Hidden : SidebarMode.Full));

        // If we left Narrow mode, make sure the drawer + scrim are closed.
        if (!isNarrow) CloseDrawer();
    }

    private ShellMode ResolveShellState(double width)
    {
        // Force idioms to their natural shell state regardless of pixel width
        // (a phone in landscape still gets the drawer; a tablet still gets the rail).
        var idiom = DeviceInfo.Idiom;
        if (idiom == DeviceIdiom.Phone) return ShellMode.Narrow;
        if (idiom == DeviceIdiom.Tablet) return ShellMode.Medium;

        if (width >= WideBreakpoint) return ShellMode.Wide;
        if (width >= MediumBreakpoint) return ShellMode.Medium;
        return ShellMode.Narrow;
    }

    private void OnHamburger(object? sender, EventArgs e) => OpenDrawer();

    private void OnScrimTapped(object? sender, EventArgs e) => CloseDrawer();

    private void OpenDrawer()
    {
        Scrim.IsVisible = true;
        Drawer.IsVisible = true;
        // Animate scrim + drawer in. Use simple translation/opacity transitions;
        // MAUI's ViewExtensions handle these cheaply on all platforms.
        Scrim.FadeTo(1, length: 150);
        Drawer.TranslateTo(0, 0, length: 200, easing: Easing.CubicOut);
    }

    private void CloseDrawer()
    {
        if (!Drawer.IsVisible) return;
        Drawer.TranslateTo(-Drawer.Width, 0, length: 150, easing: Easing.CubicIn);
        Scrim.FadeTo(0, length: 150);
        // Hide after the animation completes so the views don't grab input while
        // animating off-screen.
        _ = Task.Delay(160).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Drawer.IsVisible = false;
                Scrim.IsVisible = false;
            });
        });
    }

    private enum ShellMode { Wide, Medium, Narrow }
}
