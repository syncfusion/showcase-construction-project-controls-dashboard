using Microsoft.AspNetCore.Components;

namespace Construction.Blazor.Components.Layout;

public partial class MainLayout
{
    [Inject] private ThemeInterop ThemeInterop { get; set; } = default!;

    private string _theme = "light";
    private bool _menuOpen;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Blazor Server renders the first paint on the server with no access to the
            // browser's localStorage/prefers-color-scheme, so the real theme is only known
            // once the interactive circuit connects — this one extra render is expected.
            _theme = await ThemeInterop.GetInitialThemeAsync();
            await ThemeInterop.SetHtmlThemeAsync(_theme);
            await ThemeInterop.SetSyncfusionThemeAsync(_theme == "dark");
            StateHasChanged();
        }
    }

    private async Task ToggleTheme()
    {
        _theme = _theme == "light" ? "dark" : "light";
        await ThemeInterop.SetStoredThemeAsync(_theme);
        await ThemeInterop.SetHtmlThemeAsync(_theme);
        // Syncfusion's own theme (Grid, Chart, Schedule, Maps, Diagram, PdfViewer chrome) is a
        // separate static stylesheet that doesn't respond to our data-theme attribute at all,
        // so it has to be swapped explicitly alongside our own theme toggle.
        await ThemeInterop.SetSyncfusionThemeAsync(_theme == "dark");
    }

    private void ToggleMenu() => _menuOpen = !_menuOpen;

    private void CloseMenu() => _menuOpen = false;
}
