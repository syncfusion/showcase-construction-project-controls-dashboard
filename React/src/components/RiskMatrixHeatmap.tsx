import { type ReactElement, useEffect, useMemo, useState } from 'react';
import { HeatMapComponent } from '@syncfusion/ej2-react-heatmap';
import type {
  RiskMatrixCellViewModel,
  RiskProbability,
  RiskSeverity,
} from '../types';
import { getThemeScopeElement } from '../utils/theme';

/**
 * Risk Matrix rendered with the Syncfusion React HeatMap component.
 *
 * Visually reproduces the hand-coded matrix defined in the design screen
 * `.designs/screens/risks.html` exactly. No Syncfusion theme CSS is imported;
 * every color/dimension is driven from the project's token system
 * (`--color-{success,info,warning,error}{,-background}`, `--color-secondary`,
 * `--radius-md`, `--space-xs`) and applied via component props + a one-shot
 * `loaded` post-processing of the rendered SVG for per-cell label colors.
 *
 * Layout parity with the design HTML:
 *  - rows: probabilities top→bottom High, Medium, Low (matches the design order)
 *  - columns: severities Low, Medium, High, Critical (left→right)
 *  - cell label: the FIRST risk number in the bucket (single "R-xxxx")
 *  - cell background: 4 discrete token-backed pastel tones by risk bucket
 *  - cell label color: the matching saturated token foreground for that tone
 *    (e.g. green cell on `--color-success-background` → label in `--color-success`)
 *  - critical-bucket label is bold (font-weight 700)
 *  - cell corner radius: --radius-md (6px); cell gap: --space-xs (4px)
 *  - axis labels: --color-secondary
 *  - legend and tooltip are disabled
 */
export interface RiskMatrixHeatmapProps {
  matrix: RiskMatrixCellViewModel[];
  /** Called when a cell is clicked; receives the probability and severity of that cell. */
  onCellClick?: (probability: RiskProbability, severity: RiskSeverity) => void;
}

const probabilitiesTopDown: RiskProbability[] = ['High', 'Medium', 'Low'];
const severitiesLeftRight: RiskSeverity[] = ['Low', 'Medium', 'High', 'Critical'];

type Tone = 'positive' | 'info' | 'warning' | 'negative';

// Minimal shape of the ej2 HeatMap instance exposed on the `loaded` event
// (ILoadedEventArgs.heatmap). We only need the root element to post-process
// per-cell label colors, so a structural type avoids importing the full class.
interface HeatMapLoadedLike {
  element?: HTMLElement;
}

// Reproduces the matrixTone() rule used by the design HTML exactly so every
// cell keeps the same tone — and therefore the same background + foreground
// color pair — as the design.
function matrixTone(severity: RiskSeverity, probability: RiskProbability): Tone {
  if (severity === 'Critical' || (severity === 'High' && probability === 'High')) return 'negative';
  if (severity === 'High' || probability === 'High' || (severity === 'Medium' && probability === 'Medium')) return 'warning';
  if (severity === 'Low') return 'positive';
  return 'info';
}

// Discrete numeric score per tone — maps each cell to a palette entry via
// paletteSettings.type = 'Fixed'. The numbers themselves never appear in the UI.
const toneScore: Record<Tone, number> = {
  positive: 0,
  info: 1,
  warning: 2,
  negative: 3,
};

// Resolve a CSS custom property on the theme-scoped element (`.app-shell`,
// where `data-theme` actually lives — see getThemeScopeElement) to a
// literal color string. Must be called at render/event time (NOT memoized
// without a theme dependency) so the resolved value reflects the current
// light/dark mode tokens.
function resolveToken(name: string, fallback = '#cccccc'): string {
  if (typeof window === 'undefined') return fallback;
  const value = getComputedStyle(getThemeScopeElement()).getPropertyValue(name).trim();
  return value || fallback;
}

// Resolve a token to a number (pixels).
function resolveTokenPx(name: string, fallbackPx: number): number {
  if (typeof window === 'undefined') return fallbackPx;
  const raw = getComputedStyle(getThemeScopeElement()).getPropertyValue(name).trim();
  if (!raw) return fallbackPx;
  if (raw.endsWith('px')) return Number.parseFloat(raw.slice(0, -2)) || fallbackPx;
  const num = Number.parseFloat(raw);
  return Number.isFinite(num) ? num : fallbackPx;
}

interface ToneColors {
  // Segment background — light pastel from the project's `-background` tokens.
  bg: string;
  // Label foreground — the matching saturated tone color (`--color-<tone>`),
  // exactly what the design HTML sets as the cell text color (NOT a darkened
  // background; the design uses the saturated tone token directly).
  fg: string;
}

function resolveToneColors(): Record<Tone, ToneColors> {
  return {
    positive: {
      bg: resolveToken('--color-success-background', '#ecfdf3'),
      fg: resolveToken('--color-success', '#12b76a'),
    },
    info: {
      bg: resolveToken('--color-info-background', '#eff8ff'),
      fg: resolveToken('--color-info', '#175cd3'),
    },
    warning: {
      bg: resolveToken('--color-warning-background', '#fffaeb'),
      fg: resolveToken('--color-warning', '#dc6803'),
    },
    negative: {
      bg: resolveToken('--color-error-background', '#fef3f2'),
      fg: resolveToken('--color-error', '#d92c20'),
    },
  };
}

export function RiskMatrixHeatmap({ matrix, onCellClick }: RiskMatrixHeatmapProps): ReactElement {
  // ── Theme-change reactivity ────────────────────────────────────────────────
  // When the user toggles dark mode, the app mutates a class on <html>
  // (e.g. adds/removes "dark"). We watch that attribute and bump a counter so
  // this component re-renders, which causes all token resolutions below to run
  // again with the updated CSS custom-property values.
  const [themeRevision, setThemeRevision] = useState(0);
  useEffect(() => {
    const bump = () => setThemeRevision((n) => n + 1);
    // The token resolution driven by this counter runs via useMemo during
    // the render phase, so the very first pass (themeRevision === 0)
    // happens before this component's own tree — including `.app-shell` —
    // has actually been committed to the DOM, and getThemeScopeElement()
    // falls back to `document.documentElement` for that one pass. Bumping
    // once here (effects run after commit) forces a second, correct pass
    // now that `.app-shell` genuinely exists.
    bump();
    // Watch explicit class/data-theme toggles (e.g. in-app theme switcher)
    const observer = new MutationObserver(bump);
    observer.observe(getThemeScopeElement(), { attributes: true, attributeFilter: ['class', 'data-theme'] });
    // Also watch the OS-level prefers-color-scheme media query so a system
    // dark/light switch (with no class change) also remounts the heatmap.
    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    mq.addEventListener('change', bump);
    return () => {
      observer.disconnect();
      mq.removeEventListener('change', bump);
    };
  }, []);

  // ── Token resolution (re-runs on every render, incl. theme toggles) ────────
  // All CSS custom-property lookups are intentionally NOT wrapped in useMemo
  // so they always reflect the current light/dark-mode token values.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const toneColors = useMemo(resolveToneColors, [themeRevision]);
  // Design system font token (--font-sans from tokens.css).
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const fontFamily = useMemo(() => resolveToken('--font-sans', '"Inter", system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif'), [themeRevision]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const pageBackground = useMemo(() => resolveToken('--color-background', '#ffffff'), [themeRevision]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const axisLabelColor = useMemo(() => resolveToken('--color-secondary', '#475467'), [themeRevision]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const cellRadius = useMemo(() => resolveTokenPx('--radius-md', 6), [themeRevision]);
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const cellGapWidth = useMemo(() => resolveTokenPx('--space-xs', 4), [themeRevision]);

  // ── Lookup: probability-severity → cell view model ─────────────────────────
  const lookup = useMemo(() => {
    const map = new Map<string, RiskMatrixCellViewModel>();
    matrix.forEach((cell) => map.set(`${cell.probability}-${cell.severity}`, cell));
    return map;
  }, [matrix]);

  // ── Numeric score matrix ───────────────────────────────────────────────────
  // Rows = probabilities top→down, cols = severities left→right.
  // Drives palette mapping; cellRender supplies the visible label.
  const dataSource = useMemo(
    () =>
      probabilitiesTopDown.map((probability) =>
        severitiesLeftRight.map((severity) =>
          toneScore[matrixTone(severity, probability)],
        ),
      ),
    [],
  );

  // Fixed palette by tone score — type: 'Fixed' maps each score to an exact
  // color (no interpolation), which is what we need for the 4-bucket matrix.
  // Re-derived on every theme revision so dark-mode bg tokens take effect.
  const palette = useMemo(
    () => [
      { value: toneScore.positive, color: toneColors.positive.bg, label: 'Low' },
      { value: toneScore.info,     color: toneColors.info.bg,     label: 'Medium' },
      { value: toneScore.warning,  color: toneColors.warning.bg,  label: 'High' },
      { value: toneScore.negative, color: toneColors.negative.bg, label: 'Critical' },
      // Sentinel: ej2 Fixed palette treats each entry as a range-start;
      // without an upper boundary the last real value gets #EEEEEE fallback.
      // This extra entry at score+1 ensures the negative range [3,4) is covered.
      { value: toneScore.negative + 1, color: toneColors.negative.bg, label: '' },
    ],
    [toneColors],
  );

  // Per-cell specs (background + foreground color + bold flag), row-major order.
  // Re-derived when toneColors change (i.e. on theme toggle).
  // We force-apply bg in the `loaded` handler too because ej2's Fixed palette
  // maps the last range open-ended and falls back to #EEEEEE for cells whose
  // value equals the final palette entry value.
  const cellSpecs = useMemo(() => {
    const specs: { bg: string; fg: string; bold: boolean }[] = [];
    probabilitiesTopDown.forEach((probability) => {
      severitiesLeftRight.forEach((severity) => {
        const tone = matrixTone(severity, probability);
        specs.push({
          bg: toneColors[tone].bg,
          fg: toneColors[tone].fg,
          bold: tone === 'negative',
        });
      });
    });
    return specs;
  }, [toneColors]);

  return (
    <div className="risk-matrix-heatmap">
      <HeatMapComponent
        // key forces ej2 to fully remount on theme toggle so the palette
        // (which ej2 only reads once on initialization) picks up the updated
        // dark-mode background token values resolved above.
        key={themeRevision}
        id="risk-matrix-heatmap"
        dataSource={dataSource}
        // Compact fixed height so ej2 doesn't auto-fill to its 450px default.
        // ~3 cell rows (45px each) + axis row (21px) + 4px gaps ≈ 160px, which
        // matches the design's gridTemplateRows ("45px 45px 45px 21px").
        height="180px"
        width="100%"
        margin={{ left: 0, right: 0, top: 0, bottom: 0 }}
        xAxis={{
          labels: severitiesLeftRight,
          // Without explicit minimum/maximum, ej2 derives axis size from
          // dataSource "maxLength" which doesn't reliably track our 4
          // severities. Setting minimum=0, maximum=3 forces 4 column ticks
          // (0..3) so all four severity labels (Low/Medium/High/Critical)
          // render and the dataSource's outer/inner shape matches.
          valueType: 'Category',
          minimum: 0,
          maximum: 3,
          opposedPosition: true,
          textStyle: {
            size: '14px',
            color: axisLabelColor,
            fontFamily: fontFamily,
          },
        }}
        yAxis={{
          labels: probabilitiesTopDown,
          valueType: 'Category',
          minimum: 0,
          maximum: 2,
          opposedPosition: false,
          textStyle: {
            size: '14px',
            color: axisLabelColor,
            fontFamily: fontFamily,
          },
        }}
        paletteSettings={{ type: 'Fixed', palette }}
        cellSettings={{
          showLabel: true,
          textStyle: {
            size: '14px',
            color: axisLabelColor,
            fontFamily: fontFamily,
          },
          border: { width: cellGapWidth, color: pageBackground, radius: cellRadius },
        }}
        legendSettings={{ visible: false }}
        showTooltip={false}
        loaded={(args: { heatmap?: HeatMapLoadedLike }) => {
          // ej2's `cellRender` arg (ICellEventArgs) exposes no writable
          // per-cell label color, so we post-process the rendered SVG here.
          // This handler runs after every render (incl. theme toggles), so all
          // token values used inside are freshly resolved via the closed-over
          // cellLabelSpec (which itself depends on toneColors, re-resolved on
          // each themeRevision bump by the MutationObserver above).
          //
          // Per-cell label <text> ids: "..._HeatMapRectLabels_<n>"
          // Per-cell rect  <rect> ids: "..._HeatMapRect_<n>"
          const root = args.heatmap?.element;
          if (!root) return;

          // ej2 renders a <rect id="..._HeatmapBorder"> with a hardcoded
          // white fill as the component background. CSS `background: transparent`
          // cannot reach an SVG fill attribute, so we clear it here so the card
          // background shows through in both light and dark mode.
          const border = root.querySelector<SVGRectElement>('[id$="_HeatmapBorder"]');
          if (border) {
            border.setAttribute('fill', 'transparent');
          }

          // Force-apply bg on every cell rect. ej2's Fixed palette maps the
          // last range as open-ended and falls back to #EEEEEE for cells whose
          // value == the final palette entry value, so we override all fills
          // directly to guarantee the correct token color in light AND dark mode.
          const rects = root.querySelectorAll<SVGRectElement>('[id*="_HeatMapRect_"]');
          rects.forEach((rect) => {
            const m = rect.id.match(/_HeatMapRect_(\d+)$/);
            if (!m) return;
            const idx = Number.parseInt(m[1], 10);
            const spec = cellSpecs[idx];
            if (spec) rect.setAttribute('fill', spec.bg);
            rect.setAttribute('stroke', pageBackground);
          });

          // Apply saturated fg color + bold weight to each cell label.
          const labels = root.querySelectorAll<SVGTextElement>('[id*="_HeatMapRectLabels_"]');
          labels.forEach((label) => {
            const m = label.id.match(/_HeatMapRectLabels_(\d+)$/);
            if (!m) return;
            const idx = Number.parseInt(m[1], 10);
            const spec = cellSpecs[idx];
            if (!spec) return;
            label.setAttribute('fill', spec.fg);
            label.style.fill = spec.fg;
            label.style.fontFamily = fontFamily;
            label.style.fontWeight = spec.bold ? '700' : '400';
          });
        }}
        cellRender={(args: {
          value: number;
          xLabel: string;
          yLabel: string;
          displayText: string;
        }) => {
          // ej2 ICellEventArgs uses xLabel / yLabel. Per the design, each cell
          // shows the FIRST risk number in the bucket (single "R-xxxx"), not a
          // joined/overflow label. Empty cells render no label.
          const severity = args.xLabel as RiskSeverity;
          const probability = args.yLabel as RiskProbability;
          const cell = lookup.get(`${probability}-${severity}`);
          args.displayText = cell && cell.riskIds.length > 0 ? cell.riskIds[0] : '';
        }}
        cellClick={(args: { xLabel: string; yLabel: string }) => {
          if (!onCellClick) return;
          const severity = args.xLabel as RiskSeverity;
          const probability = args.yLabel as RiskProbability;
          // Only fire if this cell has at least one risk
          const cell = lookup.get(`${probability}-${severity}`);
          if (cell && cell.riskIds.length > 0) {
            onCellClick(probability, severity);
          }
        }}
      />
    </div>
  );
}
