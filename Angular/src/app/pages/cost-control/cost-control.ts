/**
 * Cost Control page — portfolio-level budget, earned value, change orders.
 *
 * Syncfusion components used:
 *   - `<ejs-grid>` for the change-orders register (sort + page + resize).
 *   - `<ejs-chart>` Column series for the Earned Value bar chart
 *     (planned vs actual by month, theme-aware palette via SyncfusionTokens).
 *   - `<ejs-heatmap>` for the Cost Variance HeatMap (3 discrete tones
 *     mapped from variance %, foreground colour matches bold label colour
 *     in the React app's CSS).
 *
 * No Syncfusion theme CSS is imported — every chart is styled entirely
 * from the in-house token CSS via resolveTokenColor() / SyncfusionTokens.
 */
import { Component, OnInit, signal, computed, effect, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ReportsService } from '../../core/services/reports.service';
import { ChangeOrdersService } from '../../core/services/change-orders.service';
import type { ChangeOrderStatus, ChangeOrderSummaryDto, CostKpisDto, CostPerformancePointDto, CostVarianceByCostCodeDto } from '../../core/models/api.models';
import { Modal } from '../../shared/components/modal/modal';
import { IconComponent } from '../../shared/components/icon';
import { downloadCsv } from '../../shared/utils/csv';
import { formatDate } from '../../shared/utils/date.util';
import { formatCompactCurrency, formatCurrency } from '../../shared/utils/format.util';
import { GridModule, PageService, ResizeService } from '@syncfusion/ej2-angular-grids';
import type { RowSelectEventArgs } from '@syncfusion/ej2-grids';
import {
  ChartModule,
  ColumnSeriesService,
  CategoryService,
  TooltipService,
} from '@syncfusion/ej2-angular-charts';
import { HeatMapModule } from '@syncfusion/ej2-angular-heatmap';
import {
  SyncfusionTokensService,
  resolveTokenColor,
} from '../../core/utils/syncfusion-tokens';
import { MeasuredWidthDirective } from '../../shared/directives/measured-width.directive';

const FALLBACK_COST_CODES: CostVarianceByCostCodeDto[] = [
  { costCode: 'General Conditions', variancePct: 2 },
  { costCode: 'Sitework', variancePct: 4 },
  { costCode: 'Concrete', variancePct: -3 },
  { costCode: 'Masonry', variancePct: -5 },
  { costCode: 'Metals', variancePct: -8 },
  { costCode: 'Finishes', variancePct: 1 },
];

const statusBadgeClass: Record<ChangeOrderStatus, string> = {
  Draft: 'badge-neutral',
  Submitted: 'badge-info',
  UnderReview: 'badge-warning',
  Approved: 'badge-success',
  Rejected: 'badge-error',
};

const statusLabel: Record<ChangeOrderStatus, string> = {
  Draft: 'Draft',
  Submitted: 'Submitted',
  UnderReview: 'Review',
  Approved: 'Approved',
  Rejected: 'Rejected',
};

const coStatusOptions: (ChangeOrderStatus | 'All')[] = ['All', 'Draft', 'Submitted', 'UnderReview', 'Approved', 'Rejected'];

interface NewChangeOrderDraft {
  projectId: string;
  description: string;
  amount: string;
  requestedBy: string;
  requestDate: string;
  impactDays: string;
  justification: string;
}

function emptyChangeOrderDraft(): NewChangeOrderDraft {
  const today = new Date().toISOString().slice(0, 10);
  return { projectId: '', description: '', amount: '', requestedBy: '', requestDate: today, impactDays: '', justification: '' };
}

function pctOfBudget(spend: number, budget: number): string {
  if (!budget) return '—';
  return `${Math.round((spend / budget) * 100)}% of budget`;
}

/**
 * Map a variance percentage to a discrete tone for the heatmap:
 *   positive  → variance ≥  0%  (good — green)
 *   warning   → variance ≥ -5%  (watch — yellow)
 *   negative  → variance <  -5%  (bad — red)
 */
type VarianceTone = 'positive' | 'warning' | 'negative';

function varianceTone(variancePct: number): VarianceTone {
  if (variancePct >= 0) return 'positive';
  if (variancePct >= -5) return 'warning';
  return 'negative';
}

const VARIANCE_TONE_SCORE: Record<VarianceTone, number> = {
  positive: 0,
  warning: 1,
  negative: 2,
};

@Component({
  selector: 'app-cost-control',
  standalone: true,
  imports: [GridModule, Modal, IconComponent, ChartModule, HeatMapModule, MeasuredWidthDirective],
  providers: [
    PageService,
    ResizeService,
    ColumnSeriesService,
    CategoryService,
    TooltipService,
  ],
  templateUrl: './cost-control.html',
  styleUrl: './cost-control.css',
})
export class CostControl implements OnInit {
  readonly statusBadgeClass = statusBadgeClass;
  readonly statusLabel = statusLabel;
  readonly coStatusOptions = coStatusOptions;
  readonly formatDate = formatDate;
  readonly formatCurrency = formatCurrency;
  readonly formatCompactCurrency = formatCompactCurrency;

  // Template-side lookups (Syncfusion's <ng-template let-co> context is `any`,
  // so `statusBadgeClass[co.status]` triggers TS7053 under strict template
  // checking). These wrappers accept a `string` and cast on the TS side.
  statusBadge(s: string): string { return statusBadgeClass[s as ChangeOrderStatus] ?? ''; }
  statusLabelFor(s: string): string { return statusLabel[s as ChangeOrderStatus] ?? s; }

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tokens = inject(SyncfusionTokensService);

  // Theme-bridge palette — re-resolved on every token revision so chart
  // colours track light/dark mode toggles.
  readonly chartPalette = computed(() => {
    void this.tokens.revision();
    return {
      axisLabel: resolveTokenColor('--color-secondary', '#475467'),
      axisLine: resolveTokenColor('--color-border', '#eaecf0'),
      tooltipBg: resolveTokenColor('--color-primary', '#101828'),
      tooltipText: resolveTokenColor('--color-background', '#ffffff'),
      // Syncfusion SVG expects literal font/colour values — never `var(--...)`.
      // We resolve the project's font/size tokens at runtime so dark mode plus
      // any future token edits still flow through to the chart text styles.
      fontFamily: resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif'),
      captionSize: resolveTokenColor('--text-caption-size', '12px'),
      planned: resolveTokenColor('--color-accent', '#2563eb'),
      actual: resolveTokenColor('--color-success', '#12b76a'),
      positiveBg: resolveTokenColor('--color-success-background', '#ecfdf3'),
      positiveFg: resolveTokenColor('--color-success', '#12b76a'),
      warningBg: resolveTokenColor('--color-warning-background', '#fffaeb'),
      warningFg: resolveTokenColor('--color-warning', '#dc6803'),
      negativeBg: resolveTokenColor('--color-error-background', '#fef3f2'),
      negativeFg: resolveTokenColor('--color-error', '#d92c20'),
      foreground: resolveTokenColor('--color-primary', '#101828'),
    };
  });

  // Earned-value series for `<ejs-chart>` Column series.
  readonly trendSeriesData = computed(() => {
    void this.tokens.revision();
    return this.trend().map((p) => ({ x: p.month, planned: p.planned, actual: p.actual }));
  });

  // HeatMap datasource: a single row containing one numeric tone-score per cost code.
  readonly heatmapData = computed(() => {
    void this.tokens.revision();
    return [this.variances().map((v) => VARIANCE_TONE_SCORE[varianceTone(v.variancePct)])];
  });

  // Per-cell specs (row-major) used by the heatmap's `loaded` event
  // handler to force-apply bg colour (ej2's Fixed palette leaves the
  // last range open-ended).
  readonly heatmapCellSpecs = computed(() => {
    void this.tokens.revision();
    return this.variances().map((v) => ({
      costCode: v.costCode,
      variancePct: v.variancePct,
      tone: varianceTone(v.variancePct),
    }));
  });

  kpis = signal<CostKpisDto | null>(null);
  trend = signal<CostPerformancePointDto[]>([]);
  variances = signal<CostVarianceByCostCodeDto[]>([]);
  changeOrders = signal<ChangeOrderSummaryDto[]>([]);
  coSearch = signal('');
  coStatus = signal<ChangeOrderStatus | 'All'>('UnderReview');
  coLoading = signal(false);
  loading = signal(true);
  error = signal<string | null>(null);
  selectedCo = signal<ChangeOrderSummaryDto | null>(null);
  showNewCoModal = signal(false);
  newCoDraft = signal<NewChangeOrderDraft>(emptyChangeOrderDraft());

  budgetDelta = computed(() => {
    const kpis = this.kpis();
    return kpis ? kpis.totalPortfolioBudget - kpis.forecastAtCompletion : null;
  });

  filteredChangeOrders = computed(() => {
    const q = this.coSearch().trim().toLowerCase();
    const status = this.coStatus();
    return this.changeOrders().filter((co) => {
      const matchesStatus = status === 'All' || co.status === status;
      const matchesSearch =
        !q ||
        co.number.toLowerCase().includes(q) ||
        co.description.toLowerCase().includes(q) ||
        String(co.projectId).toLowerCase().includes(q);
      return matchesStatus && matchesSearch;
    });
  });

  constructor(private reports: ReportsService, private changeOrdersApi: ChangeOrdersService) {
    // Read ?coStatus=UnderReview from URL, mirror React's URLSourceOfTruth pattern.
    const param = this.route.snapshot.queryParamMap.get('coStatus');
    if (param && (coStatusOptions as string[]).includes(param)) {
      this.coStatus.set(param as ChangeOrderStatus | 'All');
    }
    // Push filter state to URL whenever the user picks a different filter.
    effect(() => {
      const next = this.coStatus();
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { coStatus: next === 'All' ? null : next },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    });
  }

  ngOnInit(): void {
    forkJoin([this.reports.getCostKpis(), this.reports.getCostPerformanceTrend(7), this.reports.getCostVarianceByCostCode()]).subscribe({
      next: ([kpis, trend, variances]) => {
        this.kpis.set(kpis);
        this.trend.set(trend);
        this.variances.set(variances.length ? variances : FALLBACK_COST_CODES);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err instanceof Error ? err.message : 'Failed to load cost control data');
        this.loading.set(false);
      },
    });

    this.coLoading.set(true);
    this.changeOrdersApi.getChangeOrders({ page: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.changeOrders.set(result.data);
        this.coLoading.set(false);
      },
      error: () => {
        this.changeOrders.set([]);
        this.coLoading.set(false);
      },
    });

    this.tokens.init();
  }

  pctOfBudget(spend: number, budget: number): string {
    return pctOfBudget(spend, budget);
  }

  onSearchChange(value: string): void {
    this.coSearch.set(value);
  }

  onStatusChange(value: ChangeOrderStatus | 'All'): void {
    this.coStatus.set(value);
  }

  onRowSelected(args: RowSelectEventArgs): void {
    const data = args.data as ChangeOrderSummaryDto | undefined;
    if (data) this.selectedCo.set(data);
  }

  openNewChangeOrderModal(): void {
    this.newCoDraft.set(emptyChangeOrderDraft());
    this.showNewCoModal.set(true);
  }

  updateDraft(patch: Partial<NewChangeOrderDraft>): void {
    this.newCoDraft.update((d) => ({ ...d, ...patch }));
  }

  handleSaveNewChangeOrder(): void {
    const draft = this.newCoDraft();
    if (!draft.description.trim()) return;
    const changeOrders = this.changeOrders();
    const nextId = changeOrders.length ? Math.max(...changeOrders.map((co) => co.id)) + 1 : 1;
    const created: ChangeOrderSummaryDto = {
      id: nextId,
      projectId: Number(draft.projectId) || 0,
      number: `CO-${String(nextId).padStart(4, '0')}`,
      description: draft.description.trim(),
      amount: Number(draft.amount) || 0,
      status: 'Draft',
      requestedBy: draft.requestedBy.trim() || undefined,
      requestDate: draft.requestDate || undefined,
      justification: draft.justification.trim() || undefined,
      impactDays: draft.impactDays ? Number(draft.impactDays) : undefined,
      createdDate: new Date().toISOString(),
    };
    // Demo only: kept in local component state so it's visible immediately;
    // nothing is written back to the API.
    this.changeOrders.set([created, ...changeOrders]);
    this.coStatus.set('All');
    this.coSearch.set('');
    this.showNewCoModal.set(false);
  }

  handleExportChangeOrders(): void {
    downloadCsv<ChangeOrderSummaryDto>(
      'change-orders',
      [
        { header: 'CO #', value: (co) => co.number },
        { header: 'Project ID', value: (co) => co.projectId },
        { header: 'Description', value: (co) => co.description },
        { header: 'Submitted', value: (co) => (co.requestDate ? formatDate(co.requestDate) : '') },
        { header: 'Amount', value: (co) => co.amount },
        { header: 'Schedule Impact (days)', value: (co) => co.impactDays ?? '' },
        { header: 'Status', value: (co) => statusLabel[co.status] },
      ],
      this.filteredChangeOrders(),
    );
  }

  // ── HeatMap configuration ──────────────────────────────────────────────────
  // Token-driven palette / axes / cell chrome — re-computed every theme
  // revision so light/dark mode flips propagate to the SVG.

  /** Cost-code labels along the X axis (one per variance entry). */
  readonly heatmapXAxis = computed(() => {
    void this.tokens.revision();
    return {
      labels: this.variances().map((v) => v.costCode),
      valueType: 'Category',
      opposedPosition: true,
      textStyle: {
        size: '12px',
        color: resolveTokenColor('--color-secondary', '#475467'),
        fontFamily: resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif'),
      },
    };
  });

  /** Single-row Y axis — show "Variance" label so the heatmap is anchored. */
  readonly heatmapYAxis = computed(() => ({
    labels: ['Variance'],
    opposedPosition: false,
    textStyle: {
      size: '12px',
      color: resolveTokenColor('--color-secondary', '#475467'),
      fontFamily: resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif'),
    },
  }));

  /**
   * Fixed palette: each score (0/1/2 for positive/warning/negative) maps
   * to a discrete token-backed background colour. A sentinel entry at
   * score 3 ensures ej2's open-ended range fallback doesn't kick in.
   */
  readonly heatmapPalette = computed(() => {
    void this.tokens.revision();
    const p = this.chartPalette();
    return {
      type: 'Fixed' as const,
      palette: [
        { value: VARIANCE_TONE_SCORE.positive, color: p.positiveBg },
        { value: VARIANCE_TONE_SCORE.warning, color: p.warningBg },
        { value: VARIANCE_TONE_SCORE.negative, color: p.negativeBg },
        { value: VARIANCE_TONE_SCORE.negative + 1, color: p.negativeBg },
      ],
    };
  });

  /** Cell chrome: show label, set default font, gap = border, radius. */
  readonly heatmapCellSettings = computed(() => {
    void this.tokens.revision();
    return {
      showLabel: true,
      textStyle: {
        size: '12px',
        color: resolveTokenColor('--color-secondary', '#475467'),
        fontFamily: resolveTokenColor('--font-sans', '"Inter", system-ui, sans-serif'),
      },
      border: {
        width: 4,
        color: resolveTokenColor('--color-background', '#ffffff'),
        radius: 6,
      },
    };
  });

  /**
   * `cellRender` for the heatmap — inject per-cell LABEL text
   * (cost code + signed variance %) and force-apply the foreground
   * colour matching the tone (mirrors the RiskMatrixHeatmap pattern).
   */
  onHeatmapCellRender(args: {
    value: number;
    xLabel: string;
    yLabel: string;
    displayText: string;
    cellColor: string;
  }): void {
    const xLabel = args.xLabel;
    const specs = this.heatmapCellSpecs();
    const match = specs.find((s) => s.costCode === xLabel);
    if (!match) return;
    const sign = match.variancePct >= 0 ? '+' : '';
    args.displayText = `${sign}${match.variancePct}%`;
    const palette = this.chartPalette();
    const toneToFg: Record<VarianceTone, string> = {
      positive: palette.positiveFg,
      warning: palette.warningFg,
      negative: palette.negativeFg,
    };
    args.cellColor = toneToFg[match.tone];
  }

  /**
   * `loaded` event — clear ej2's hardcoded white background rect so the
   * card's surface shows through in light/dark mode.
   */
  onHeatmapLoaded(args: { heatmap?: { element?: HTMLElement } }): void {
    const root = args.heatmap?.element;
    if (!root) return;
    const border = root.querySelector<SVGRectElement>('[id$="_HeatmapBorder"]');
    if (border) border.setAttribute('fill', 'transparent');
  }
}

