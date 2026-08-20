using Microsoft.JSInterop;

namespace Construction.Blazor.Core.Interop;

public class ThemeInterop(IJSRuntime js)
{
    public async Task<string> GetInitialThemeAsync()
    {
        var stored = await js.InvokeAsync<string?>("themeInterop.getStoredTheme");
        if (stored is "dark" or "light") return stored;
        var prefersDark = await js.InvokeAsync<bool>("themeInterop.prefersDark");
        return prefersDark ? "dark" : "light";
    }

    public ValueTask SetStoredThemeAsync(string theme) =>
        js.InvokeVoidAsync("themeInterop.setStoredTheme", theme);

    public ValueTask SetSyncfusionThemeAsync(bool isDark) =>
        js.InvokeVoidAsync("themeInterop.setSyncfusionTheme", isDark);

    public ValueTask SetHtmlThemeAsync(string theme) =>
        js.InvokeVoidAsync("themeInterop.setHtmlTheme", theme);
}
