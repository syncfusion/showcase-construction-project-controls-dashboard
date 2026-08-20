import { Component, EventEmitter, Input, OnInit, Output, computed, inject } from '@angular/core';
import { HeatMapModule } from '@syncfusion/ej2-angular-heatmap';
import type {
  RiskMatrixCellViewModel,
  RiskProbability,
  RiskSeverity,
} from '../../../core/models/api.models';
import {
  SyncfusionTokensService,
  resolveTokenColor,
  resolveTokenPx,
} from '../../../core/utils/syncfusion-tokens';

/**
 * Risk Matrix rendered with the Syncfusion Angular HeatMap component.
 *
 * Mirrors `frontend/Construction.React/src/components/RiskMatrixHeatmap.tsx`
 * 1:1:
 *  - rows: probabilities top→down High, Medium, Low
 *  - cols: severities left→right Low, Medium, High, Critical
 *  - cell label: FIRST risk number in the bucket, no overflow suffix
 *  - cell backgrounds & label colors: 4 discrete token-backed tones
 *  - critical-bucket label is bold (font-weight 700)
 *  - legend + tooltip disabled (parity with React)
 *  - theme re-renders: token palette re-resolved whenever
 *    `SyncfusionTokensService.revision()` flips
 *  - `loaded` handler post-processes the rendered SVG to force-apply bg/fg
 *    (ej2 Fixed palette leaves the last range open-ended and falls back to
 *    #EEEEEE for cells whose value == final palette entry value)
 */
@Component({
  selector: 'app-risk-matrix-heatmap',
  standalone: true,
  imports: [HeatMapModule],
  template: `
    <div class="risk-matrix-heatmap">
      <ejs-heatmap
        id="risk-matrix-heatmap"
        [attr.data-revision]="tokens.revision()"
        [dataSource]="dataSource()"
        height="180px"
        width="100%"
        [margin]="{ left: 0, right: 0, top: 0, bottom: 0 }"
        [xAxis]="xAxis()"
        [yAxis]="yAxis()"
        [paletteSettings]="{ type: 'Fixed', palette: palette() }"
        [cellSettings]="cellSettings()"
        [legendSettings]="{ visible: false }"
        [showTooltip]="false"
        (loaded)="onLoaded($event)"
        (cellRender)="onCellRender($event)"
        (cellClick)="onCellClick($event)"
      ></ejs-heatmap>
    </div>
  `,
  styles: [`
    .risk-matrix-heatmap {
      width: 100%;
    }
    .risk-matrix-heatmap .e-heatmap {
      background: transparent;
      border: none;
      font-family: var(--font-sans);
      font-size: var(--text-small-size);
    }
  `],
})
export class RiskMatrixHeatmapComponent implements OnInit {
  /** Tile cell data — supply `RiskMatrixCellViewModel[]` from the API. */
  @Input() matrix: RiskMatrixCellViewModel[] = [];

  /** Fired when a non-empty cell is clicked; supplies prob/severity of the cell. */
  @Output() cellClick = new EventEmitter<{ probability: RiskProbability; severity: RiskSeverity }>();

  tokens = inject(SyncfusionTokensService);

  private static readonly probabilitiesTopDown: RiskProbability[] = ['High', 'Medium', 'Low'];
  private static readonly severitiesLeftRight: RiskSeverity[] = ['Low', 'Medium', 'High', 'Critical'];

  // ── Tone → score mapping (numeric, drives Fixed palette) ────────────────
  private static matrixTone(severity: RiskSeverity, probability: RiskProbability): Tone {
    if (severity === 'Critical' || (severity === 'High' && probability === 'High')) return 'negative';
    if (severity === 'High' || probability === 'High' || (severity === 'Medium' && probability === 'Medium')) return 'warning';
    if (severity === 'Low') return 'positive';
    return 'info';
  }

  // ── Axis / palette / cellSetting objects (read token values live) ─────
  readonly xAxis = computed(() => ({
    labels: RiskMatrixHeatmapComponent.severitiesLeftRight,
    valueType: 'Category',
    minimum: 0,
    maximum: 3,
    opposedPosition: true,
    textStyle: {
      size: '14px',
      color: resolveTokenColor('--color-secondary', '#475467'),
      fontFamily: resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif'),
    },
  }));

  readonly yAxis = computed(() => ({
    labels: RiskMatrixHeatmapComponent.probabilitiesTopDown,
    valueType: 'Category',
    minimum: 0,
    maximum: 2,
    opposedPosition: false,
    textStyle: {
      size: '14px',
      color: resolveTokenColor('--color-secondary', '#475467'),
      fontFamily: resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif'),
    },
  }));

  /** Tone → {bg, fg} tokens — re-resolved on theme toggles. */
  readonly toneColors = computed(() => {
    void this.tokens.revision();
    return {
      positive: {
        bg: resolveTokenColor('--color-success-background', '#ecfdf3'),
        fg: resolveTokenColor('--color-success', '#12b76a'),
      },
      info: {
        bg: resolveTokenColor('--color-info-background', '#eff8ff'),
        fg: resolveTokenColor('--color-info', '#175cd3'),
      },
      warning: {
        bg: resolveTokenColor('--color-warning-background', '#fffaeb'),
        fg: resolveTokenColor('--color-warning', '#dc6803'),
      },
      negative: {
        bg: resolveTokenColor('--color-error-background', '#fef3f2'),
        fg: resolveTokenColor('--color-error', '#d92c20'),
      },
    };
  });

  /** Fixed palette — 4 real entries + a sentinel for ej2's open-ended range. */
  readonly palette = computed(() => {
    const c = this.toneColors();
    return [
      { value: 0, color: c.positive.bg, label: 'Low' },
      { value: 1, color: c.info.bg, label: 'Medium' },
      { value: 2, color: c.warning.bg, label: 'High' },
      { value: 3, color: c.negative.bg, label: 'Critical' },
      { value: 4, color: c.negative.bg, label: '' },
    ];
  });

  /** 3×4 numeric grid (rows × cols of score numbers). */
  readonly dataSource = computed(() => {
    void this.tokens.revision();
    return RiskMatrixHeatmapComponent.probabilitiesTopDown.map((probability) =>
      RiskMatrixHeatmapComponent.severitiesLeftRight.map((severity) =>
        scoreOf(RiskMatrixHeatmapComponent.matrixTone(severity, probability)),
      ),
    );
  });

  /** Cell chrome (border gap, radius, font family/size, default font color). */
  readonly cellSettings = computed(() => {
    void this.tokens.revision();
    const pageBackground = resolveTokenColor('--color-background', '#ffffff');
    const fontFamily = resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif');
    const cellRadius = resolveTokenPx('--radius-md', 6);
    const cellGapWidth = resolveTokenPx('--space-xs', 4);
    return {
      showLabel: true,
      textStyle: {
        size: '14px',
        color: resolveTokenColor('--color-secondary', '#475467'),
        fontFamily,
      },
      border: { width: cellGapWidth, color: pageBackground, radius: cellRadius },
    };
  });

  /** Per-cell spec list (row-major) — used by (loaded) to repaint bg/fg. */
  readonly cellSpecs = computed(() => {
    const c = this.toneColors();
    const specs: { bg: string; fg: string; bold: boolean }[] = [];
    RiskMatrixHeatmapComponent.probabilitiesTopDown.forEach((probability) => {
      RiskMatrixHeatmapComponent.severitiesLeftRight.forEach((severity) => {
        const tone = RiskMatrixHeatmapComponent.matrixTone(severity, probability);
        specs.push({
          bg: c[tone].bg,
          fg: c[tone].fg,
          bold: tone === 'negative',
        });
      });
    });
    return specs;
  });

  ngOnInit(): void {
    this.tokens.init();
  }

  /** (cellRender) — inject per-cell LABEL text (first risk number). */
  onCellRender(args: {
    value: number;
    xLabel: string;
    yLabel: string;
    displayText: string;
  }): void {
    const severity = args.xLabel as RiskSeverity;
    const probability = args.yLabel as RiskProbability;
    const cell = this.lookupCell(probability, severity);
    args.displayText = cell && cell.riskIds.length > 0 ? cell.riskIds[0] : '';
  }

  /** (cellClick) — emit only for non-empty cells, matching React parity. */
  onCellClick(args: { xLabel: string; yLabel: string }): void {
    const severity = args.xLabel as RiskSeverity;
    const probability = args.yLabel as RiskProbability;
    const cell = this.lookupCell(probability, severity);
    if (!cell || cell.riskIds.length === 0) return;
    this.cellClick.emit({ probability, severity });
  }

  /**
   * (loaded) — post-process the rendered SVG:
   *   - clear ej2's hardcoded white HeatmapBorder fill
   *   - force-apply bg fill + page-bg stroke on every cell rect
   *   - force-apply fg fill + bold weight on every cell label text
   *
   *   ej2 ICellEventArgs (used by cellRender) has no writable per-cell label
   *   colour, so this is the only reliable way to colour per-cell labels.
   */
  onLoaded(args: { heatmap?: { element?: HTMLElement } }): void {
    const root = args.heatmap?.element;
    if (!root) return;

    const pageBackground = resolveTokenColor('--color-background', '#ffffff');
    const fontFamily = resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif');
    const specs = this.cellSpecs();

    const border = root.querySelector<SVGRectElement>('[id$="_HeatmapBorder"]');
    if (border) border.setAttribute('fill', 'transparent');

    const rects = root.querySelectorAll<SVGRectElement>('[id*="_HeatMapRect_"]');
    rects.forEach((rect) => {
      const m = rect.id.match(/_HeatMapRect_(\d+)$/);
      if (!m) return;
      const idx = Number.parseInt(m[1], 10);
      const spec = specs[idx];
      if (!spec) return;
      rect.setAttribute('fill', spec.bg);
      rect.setAttribute('stroke', pageBackground);
    });

    const labels = root.querySelectorAll<SVGTextElement>('[id*="_HeatMapRectLabels_"]');
    labels.forEach((label) => {
      const m = label.id.match(/_HeatMapRectLabels_(\d+)$/);
      if (!m) return;
      const idx = Number.parseInt(m[1], 10);
      const spec = specs[idx];
      if (!spec) return;
      label.setAttribute('fill', spec.fg);
      label.style.fill = spec.fg;
      label.style.fontFamily = fontFamily;
      label.style.fontWeight = spec.bold ? '700' : '400';
      // Show pointer cursor so users know cells are clickable.
      label.style.cursor = 'pointer';
    });

    const cellRects = root.querySelectorAll<SVGRectElement>('[id*="_HeatMapRect_"]');
    cellRects.forEach((rect) => {
      rect.style.cursor = 'pointer';
    });
  }

  private lookupCell(probability: RiskProbability, severity: RiskSeverity): RiskMatrixCellViewModel | undefined {
    return this.matrix.find((c) => c.probability === probability && c.severity === severity);
  }
}

type Tone = 'positive' | 'info' | 'warning' | 'negative';

function scoreOf(tone: Tone): number {
  switch (tone) {
    case 'positive':
      return 0;
    case 'info':
      return 1;
    case 'warning':
      return 2;
    case 'negative':
      return 3;
  }
}
