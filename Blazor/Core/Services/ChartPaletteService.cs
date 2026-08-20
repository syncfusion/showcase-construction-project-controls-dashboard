using Construction.Blazor.Core.Interop;

namespace Construction.Blazor.Core.Services;

/// <summary>
/// Holds the resolved literal colours for all Syncfusion charts/grids in the
/// app. Mirrors the Angular <c>SyncfusionTokensService</c> and the React
/// <c>SyncfusionTokens</c> helper. After construction (which fetches the
/// values from <see cref="TokenColorInterop"/>), every consumer calls
/// <see cref="ResolveAsync"/> from <c>OnAfterRenderAsync</c> on the first
/// interactive render — Syncfusion SVG can't read CSS variables, so we
/// re-read <c>:root</c> each time the user toggles the theme (or a future
/// token edit) and refresh the cached palette.
/// </summary>
public class ChartPaletteService
{
    private readonly TokenColorInterop _tokens;

    public string AxisLabel { get; private set; } = "#475467";
    public string AxisLine { get; private set; } = "#eaecf0";
    public string TooltipBg { get; private set; } = "#101828";
    public string TooltipText { get; private set; } = "#ffffff";
    public string FontFamily { get; private set; } = "Inter, system-ui, sans-serif";
    public string CaptionSize { get; private set; } = "12px";

    public string Planned { get; private set; } = "#2563eb";   // --color-accent
    public string Actual { get; private set; } = "#12b76a";    // --color-success (chart bar)
    public string ActualLine { get; private set; } = "#d0d5dd"; // --color-border (donut)

    public string OnTrack { get; private set; } = "#12b76a";   // --color-success
    public string AtRisk { get; private set; } = "#dc6803";    // --color-warning
    public string Critical { get; private set; } = "#d92c20";  // --color-error
    public string NotStarted { get; private set; } = "#d0d5dd"; // --color-border

    public string PositiveBg { get; private set; } = "#ecfdf3";
    public string PositiveFg { get; private set; } = "#12b76a";
    public string WarningBg { get; private set; } = "#fffaeb";
    public string WarningFg { get; private set; } = "#dc6803";
    public string NegativeBg { get; private set; } = "#fef3f2";
    public string NegativeFg { get; private set; } = "#d92c20";
    public string InfoBg { get; private set; } = "#eff8ff";
    public string InfoFg { get; private set; } = "#175cd3";

    public string Foreground { get; private set; } = "#101828";
    public string Surface { get; private set; } = "#f9fafb";
    public string CardBackground { get; private set; } = "#ffffff";
    public string Border { get; private set; } = "#eaecf0";

    public ChartPaletteService(TokenColorInterop tokens) => _tokens = tokens;

    public async Task ResolveAsync()
    {
        AxisLabel = await _tokens.GetTokenColorAsync("--color-secondary", AxisLabel);
        AxisLine = await _tokens.GetTokenColorAsync("--color-border", AxisLine);
        TooltipBg = await _tokens.GetTokenColorAsync("--color-primary", TooltipBg);
        TooltipText = await _tokens.GetTokenColorAsync("--color-background", TooltipText);
        FontFamily = await _tokens.GetTokenColorAsync("--font-sans", FontFamily);
        CaptionSize = await _tokens.GetTokenColorAsync("--text-caption-size", CaptionSize);

        Planned = await _tokens.GetTokenColorAsync("--color-accent", Planned);
        Actual = await _tokens.GetTokenColorAsync("--color-success", Actual);
        ActualLine = await _tokens.GetTokenColorAsync("--color-border", ActualLine);

        OnTrack = await _tokens.GetTokenColorAsync("--color-success", OnTrack);
        AtRisk = await _tokens.GetTokenColorAsync("--color-warning", AtRisk);
        Critical = await _tokens.GetTokenColorAsync("--color-error", Critical);
        NotStarted = await _tokens.GetTokenColorAsync("--color-border", NotStarted);

        PositiveBg = await _tokens.GetTokenColorAsync("--color-success-background", PositiveBg);
        PositiveFg = await _tokens.GetTokenColorAsync("--color-success", PositiveFg);
        WarningBg = await _tokens.GetTokenColorAsync("--color-warning-background", WarningBg);
        WarningFg = await _tokens.GetTokenColorAsync("--color-warning", WarningFg);
        NegativeBg = await _tokens.GetTokenColorAsync("--color-error-background", NegativeBg);
        NegativeFg = await _tokens.GetTokenColorAsync("--color-error", NegativeFg);
        InfoBg = await _tokens.GetTokenColorAsync("--color-info-background", InfoBg);
        InfoFg = await _tokens.GetTokenColorAsync("--color-info", InfoFg);

        Foreground = await _tokens.GetTokenColorAsync("--color-primary", Foreground);
        Surface = await _tokens.GetTokenColorAsync("--color-surface", Surface);
        CardBackground = await _tokens.GetTokenColorAsync("--color-background", CardBackground);
        Border = await _tokens.GetTokenColorAsync("--color-border", Border);
    }
}
