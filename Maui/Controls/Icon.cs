using System.Collections.Frozen;

namespace Construction.Maui.Controls;

/// <summary>
/// Lucide icon helper. Mirrors the Blazor/React ports' use of the Lucide icon
/// font (loaded as the "Lucide" font family registered in MauiProgram.cs).
/// Usage from XAML:
///   <Image Source="{x:Static local:Icon.Dashboard}" />
/// Or from code:
///   img.Source = Icon.For("dashboard", 24, Colors.Gray900);
///
/// The font file MUST be present at <c>Resources/Fonts/lucide.ttf</c>. Download it
/// once from <c>https://unpkg.com/lucide-static@latest/font/lucide.ttf</c> and place
/// it there; the csproj already registers the "Lucide" family for it.
/// </summary>
public static class Icon
{
    // Lucide TTF glyph codepoints — the REAL ones from lucide-static's published
    // lucide.css (https://unpkg.com/lucide-static@latest/font/lucide.css), NOT
    // invented PUAs. The previous version used made-up \uea01… codes that don't
    // exist in the installed lucide.ttf, so every FontImageSource rendered a
    // blank/tofu glyph and no icons were visible anywhere in the app.
    private static readonly FrozenDictionary<string, string> Glyphs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Navigation / shell
        ["layout-dashboard"] = "\ue1c1",
        ["briefcase"]        = "\ue062",
        ["square-chart-gantt"] = "\ue169",
        ["calendar-gantt"]   = "\ue169",
        ["wallet"]           = "\ue204",
        ["alert-triangle"]   = "\ue193",
        ["bell"]             = "\ue059",
        ["map"]              = "\ue110",
        ["calendar"]         = "\ue063",
        ["file-text"]        = "\ue0cc",
        ["hard-hat"]         = "\ue0ee",
        ["users"]            = "\ue1a4",
        ["git-branch"]       = "\ue0e2",
        ["bar-chart-3"]      = "\ue2a3",
        ["pie-chart"]        = "\ue06b",
        ["settings"]         = "\ue154",
        ["menu"]             = "\ue115",
        ["x"]                = "\ue1b2",
        ["sun"]              = "\ue178",
        ["moon"]             = "\ue11e",
        ["building-2"]       = "\ue290",
        ["arrow-left"]       = "\ue048",
        ["chevron-right"]    = "\ue06f",
        ["chevron-down"]     = "\ue06d",
        ["search"]           = "\ue151",
        ["plus"]             = "\ue13d",
        ["refresh"]          = "\ue145",
        ["clock"]            = "\ue087",
        ["check-circle"]     = "\ue07c",
        ["arrow-up-right"]   = "\ue04d",
        ["arrow-down-right"] = "\ue045",
        ["download"]         = "\ue0b2",
        ["external-link"]    = "\ue0b9",
        ["more-vertical"]    = "\ue0b7",
        ["sliders-horizontal"] = "\ue29a",
    }.ToFrozenDictionary();

    /// <summary>
    /// Resolves a lucide icon name to its glyph within the Lucide font family.
    /// Unknown names return the fallback glyph (a small square).
    /// </summary>
    public static string Glyph(string name) =>
        Glyphs.TryGetValue(name ?? string.Empty, out var g) ? g : "\u25a1";

    /// <summary>Builds a <see cref="FontImageSource"/> for a Lucide icon.</summary>
    public static FontImageSource For(string name, double size = 22, Color? color = null)
    {
        var src = new FontImageSource
        {
            FontFamily = "Lucide",
            Glyph = Glyph(name),
            Size = size,
            Color = color ?? (Application.Current?.RequestedTheme == AppTheme.Dark
                ? Colors.White
                : (Color?)Application.Current?.Resources["Gray900"] ?? Colors.Black),
        };
        return src;
    }

    /// <summary>Common icon sources referenced as <c>{x:Static local:Icon.Dashboard}</c>.</summary>
    public static FontImageSource Dashboard     => For("layout-dashboard", 22);
    public static FontImageSource Projects      => For("briefcase", 22);
    public static FontImageSource Schedule      => For("square-chart-gantt", 22);
    public static FontImageSource CostControl   => For("wallet", 22);
    public static FontImageSource Risks         => For("alert-triangle", 22);
    public static FontImageSource Bell          => For("bell", 22);
    public static FontImageSource SiteMap       => For("map", 22);
    public static FontImageSource Calendar      => For("calendar", 22);
    public static FontImageSource Documents     => For("file-text", 22);
    public static FontImageSource Contractors   => For("hard-hat", 22);
    public static FontImageSource Workflows     => For("git-branch", 22);
    public static FontImageSource Reports       => For("bar-chart-3", 22);
    public static FontImageSource Settings      => For("settings", 22);
    public static FontImageSource Menu          => For("menu", 22);
    public static FontImageSource Close         => For("x", 22);
    public static FontImageSource Sun           => For("sun", 22);
    public static FontImageSource Moon          => For("moon", 22);
    public static FontImageSource Building      => For("building-2", 26);
    public static FontImageSource Back          => For("arrow-left", 20);
    public static FontImageSource ArrowUpRight  => For("arrow-up-right", 14);
    public static FontImageSource ArrowDownRight => For("arrow-down-right", 14);
    public static FontImageSource CheckCircle   => For("check-circle", 14);
    public static FontImageSource Clock         => For("clock", 14);
    public static FontImageSource Search        => For("search", 18);
    public static FontImageSource Download      => For("download", 18);
}
