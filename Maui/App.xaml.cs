using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Themes;

namespace Construction.Maui;

public partial class App : Application
{
    /// <summary>Set in MauiProgram.CreateMauiApp; used by code that builds pages dynamically.</summary>
    public static IServiceProvider Services { get; internal set; } = null!;

    /// <summary>Preferences key under which the user's chosen theme is persisted.</summary>
    public const string ThemePreferenceKey = "theme";

    /// <summary>Path to the diagnostic crash log written by the global unhandled-exception
    /// handler and by <see cref="CreateWindow"/>'s try/catch. When the app launches with no
    /// visible window, this file contains the exception that blocked window creation.
    /// Uses <see cref="Path.GetTempPath"/> (guaranteed writable) rather than
    /// <c>FileSystem.AppDataDirectory</c> so the log is written even if AppData init fails.</summary>
    public static string CrashLogPath => Path.Combine(Path.GetTempPath(), "construction-maui-crash.log");

    public App()
    {
        try
        {
            InitializeComponent();

            // Restore the user's persisted theme choice if present; otherwise fall back to
            // the OS requested theme (and default to Light when the platform reports Unknown).
            var saved = Preferences.Default.Get<string?>(ThemePreferenceKey, null);
            UserAppTheme = saved switch
            {
                nameof(AppTheme.Dark) => AppTheme.Dark,
                nameof(AppTheme.Light) => AppTheme.Light,
                _ => Application.Current?.RequestedTheme ?? AppTheme.Light,
            };
            ApplySyncfusionTheme(UserAppTheme);

            // Global unhandled-exception sink. WinUI/MAUI can silently swallow exceptions
            // thrown during startup (the process stays alive but no window appears), so route
            // every unhandled exception to a log file on disk we can inspect.
            AppDomain.CurrentDomain.UnhandledException += (_, e) => WriteCrashLog("AppDomain.UnhandledException", e.ExceptionObject);
            TaskScheduler.UnobservedTaskException += (_, e) => { WriteCrashLog("TaskScheduler.UnobservedTaskException", e.Exception); e.SetObserved(); };
        }
        catch (Exception ex)
        {
            WriteCrashLog("App.ctor", ex);
            throw;
        }
    }

    /// <summary>Appends the exception to <see cref="CrashLogPath"/> with a timestamp so the
    /// failure that blocked window creation is recoverable after the process exits.</summary>
    internal static void WriteCrashLog(string source, object? ex)
    {
        try
        {
            var msg = ex is Exception e ? $"{e.GetType().FullName}: {e.Message}\n{e}" : ex?.ToString() ?? "(null)";
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}]\n{msg}\n\n";
            File.AppendAllText(CrashLogPath, line);
        }
        catch { /* never let the logger itself throw */ }
    }

    /// <summary>Aligns the Syncfusion MAUI control theme (Material light/dark) with the
    /// current <see cref="Application.UserAppTheme"/> so Syncfusion controls render in the
    /// correct light/dark variant alongside the rest of the app.</summary>
    private void ApplySyncfusionTheme(AppTheme theme)
    {
        if (Resources.TryGetValue("SyncfusionTheme", out var rd) && rd is SyncfusionThemeResourceDictionary dict)
        {
            // Setting VisualTheme re-applies the corresponding Material light/dark resource
            // dictionaries for the Syncfusion controls (DataGrid, Charts, TabView, …).
            dict.VisualTheme = theme == AppTheme.Dark ? SfVisuals.MaterialDark : SfVisuals.MaterialLight;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var window = new Window(new NavigationPage(new Controls.AppShellLayout()));
            // Desktop-first minimums: a comfortable floor the OS can resize above.
            window.MinimumWidth = 1024;
            window.MinimumHeight = 640;

            // Launch maximized on Windows so the deployment always opens full-screen
            // (the React/Blazor ports are sized to the browser, but for the desktop
            // port users expect the app to fill the work area on first launch).
            // MAUI's `Microsoft.Maui.Controls.Window` has no `WindowState` member in
            // .NET 10, so we maximize via the native WinUI window handle: grab the
            // window's platform view once it's connected, pull the HWND, and send
            // the SC_MAXIMIZE system command via Win32 — the same maximization the
            // title-bar maximize button performs (kept the 1440x900 as the restore size).
#if WINDOWS
            window.Width = 1440;
            window.Height = 900;
            window.HandlerChanged += OnWindowHandlerChanged;
#else
            window.Width = 1440;
            window.Height = 900;
#endif
            return window;
        }
        catch (Exception ex)
        {
            WriteCrashLog("App.CreateWindow", ex);
            // Return a plain window with the error text so the user at least SEES that
            // something failed, instead of an invisible process.
            var errLabel = new Label { Text = "Failed to start Construction.Maui:\n\n" + ex, Margin = 24, LineBreakMode = LineBreakMode.WordWrap };
            return new Window(new ContentPage { Content = errLabel, Padding = 24 });
        }
    }

#if WINDOWS
    // Win32 imports for native window maximization (MAUI's Window has no
    // WindowState member in .NET 10; the only reliable way to truly maximize —
    // including the maximize button state, not just a sized-to-screen window — is
    // to post SC_MAXIMIZE to the HWND the WinUI window owns).
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SendMessage")]
    private static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(nint hWnd, int nCmdShow);

    private const int GWL_STYLE = -16;
    private const nint WS_MAXIMIZEBOX = (nint)0x00010000;
    private const uint WM_SYSCOMMAND = 0x0112;
    private const nint SC_MAXIMIZE = (nint)0xF032;
    private const int SW_MAXIMIZE = 3;

    private void OnWindowHandlerChanged(object? sender, EventArgs e)
    {
        if (sender is not Window mauiWindow || mauiWindow.Handler?.PlatformView is not Microsoft.UI.Xaml.Window winuiWindow)
            return;

        // Wire once; unsubscribe so this doesn't fire again on every handler re-attach.
        mauiWindow.HandlerChanged -= OnWindowHandlerChanged;

        try
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(winuiWindow);
            // Ensure the maximize box style is set, then send SC_MAXIMIZE.
            var style = GetWindowLongPtr(hwnd, GWL_STYLE);
            SetWindowLongPtr(hwnd, GWL_STYLE, style | WS_MAXIMIZEBOX);
            SendMessage(hwnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
            ShowWindowAsync(hwnd, SW_MAXIMIZE);
        }
        catch (Exception ex)
        {
            WriteCrashLog("App.MaximizeWindow", ex);
        }
    }
#endif

    /// <summary>Flips the app theme and persists the choice across restarts.</summary>
    public void ToggleTheme()
    {
        UserAppTheme = UserAppTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        ApplySyncfusionTheme(UserAppTheme);
        Preferences.Default.Set(ThemePreferenceKey, UserAppTheme.ToString());
    }
}