import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  ChartModule,
  AccumulationChartModule,
  ColumnSeriesService,
  CategoryService,
  TooltipService,
  PieSeriesService,
} from '@syncfusion/ej2-angular-charts';
import { GridModule, ResizeService } from '@syncfusion/ej2-angular-grids';
import { ReportsService } from '../../core/services/reports.service';
import type {
  CostKpisDto,
  CostPerformancePointDto,
  HealthStatus,
  PortfolioKpisDto,
  ProjectHealthDistributionDto,
  UpcomingMilestoneDto,
} from '../../core/models/api.models';
import { onActivateKey } from '../../shared/utils/a11y';
import { formatDate } from '../../shared/utils/date.util';
import { formatCompactCurrency } from '../../shared/utils/format.util';
import {
  SyncfusionTokensService,
  resolveTokenColor,
} from '../../core/utils/syncfusion-tokens';
import { MeasuredWidthDirective } from '../../shared/directives/measured-width.directive';
import { IconComponent } from '../../shared/components/icon';

function splitHealthLabel(status: HealthStatus): string {
  return status.replace(/([A-Z])/g, ' $1').trim();
}

const healthClasses: Record<HealthStatus, string> = {
  NotStarted: 'text-secondary',
  OnTrack: 'positive',
  AtRisk: 'warning',
  Critical: 'negative',
};

const healthBadgeClass: Record<HealthStatus, string> = {
  NotStarted: 'badge-neutral',
  OnTrack: 'badge-success',
  AtRisk: 'badge-warning',
  Critical: 'badge-error',
};

interface KpiSummaryItem {
  label: string;
  value: string;
  icon: string;
  trend: string;
  tone: 'positive' | 'negative' | 'warning';
  to?: string;
}

function formatPct(n?: number | null): string {
  if (n === undefined || n === null) return '—';
  return `${n >= 0 ? '+' : ''}${n.toFixed(1)}%`;
}

function maxCostTrend(data: CostPerformancePointDto[]): number {
  if (data.length === 0) return 1;
  return Math.max(...data.flatMap((d) => [d.planned, d.actual]), 1);
}

@Component({
  selector: 'app-dashboard',
  imports: [ChartModule, AccumulationChartModule, GridModule, MeasuredWidthDirective, IconComponent],
  providers: [
    ColumnSeriesService,
    CategoryService,
    TooltipService,
    PieSeriesService,
    ResizeService,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  readonly healthClasses = healthClasses;
  readonly healthBadgeClass = healthBadgeClass;
  readonly formatDate = formatDate;
  readonly formatCompactCurrency = formatCompactCurrency;
  readonly splitHealthLabel = splitHealthLabel;

  // Template-side lookups (Syncfusion's <ng-template let-m> context is `any`,
  // so `healthBadgeClass[m.healthStatus]` triggers TS7053 under strict template
  // checking). This wrapper accepts a `string` and casts on the TS side.
  healthBadge(s: string): string { return healthBadgeClass[s as HealthStatus] ?? ''; }

  private tokens = inject(SyncfusionTokensService);
  private router = inject(Router);

  portfolio = signal<PortfolioKpisDto | null>(null);
  cost = signal<CostKpisDto | null>(null);
  health = signal<ProjectHealthDistributionDto | null>(null);
  trend = signal<CostPerformancePointDto[]>([]);
  milestones = signal<UpcomingMilestoneDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  costScale = computed(() => maxCostTrend(this.trend()));

  // Theme-bridge palette — re-runs when tokens.revision() flips, so dark-mode
  // toggles refresh every chart color via Signal effect.
  readonly chartPalette = computed(() => {
    void this.tokens.revision();
    return {
      axisLabel: resolveTokenColor('--color-secondary', '#475467'),
      axisLine: resolveTokenColor('--color-border', '#eaecf0'),
      tooltipBg: resolveTokenColor('--color-primary', '#101828'),
      tooltipText: resolveTokenColor('--color-background', '#ffffff'),
      planned: resolveTokenColor('--color-accent', '#2563eb'),
      actual: resolveTokenColor('--color-border', '#d0d5dd'),
      onTrack: resolveTokenColor('--color-success', '#12b76a'),
      atRisk: resolveTokenColor('--color-warning', '#dc6803'),
      critical: resolveTokenColor('--color-error', '#d92c20'),
      notStarted: resolveTokenColor('--color-border', '#d0d5dd'),
      foreground: resolveTokenColor('--color-primary', '#101828'),
    };
  });

  healthCounts = computed(() => {
    const h = this.health();
    if (!h) return { onTrack: 0, atRisk: 0, critical: 0, notStarted: 0, total: 0 };
    return {
      onTrack: h.onTrack,
      atRisk: h.atRisk,
      critical: h.critical,
      notStarted: h.notStarted,
      total: h.onTrack + h.atRisk + h.critical + h.notStarted,
    };
  });

  donutBackground = computed(() => {
    void this.tokens.revision();
    const c = this.healthCounts();
    if (c.total === 0) return `conic-gradient(${this.chartPalette().axisLine} 0 100%)`;
    const t1 = (c.onTrack / c.total) * 100;
    const t2 = t1 + (c.atRisk / c.total) * 100;
    const t3 = t2 + (c.critical / c.total) * 100;
    const palette = this.chartPalette();
    return `conic-gradient(${palette.onTrack} 0% ${t1}%, ${palette.atRisk} ${t1}% ${t2}%, ${palette.critical} ${t2}% ${t3}%, ${palette.notStarted} ${t3}% 100%)`;
  });

  costTrendData = computed(() =>
    this.trend().map((p) => ({ x: p.month, planned: p.planned, actual: p.actual })),
  );

  healthDonutData = computed<{ x: string; y: number; color: string }[]>(() => {
    void this.tokens.revision();
    const c = this.healthCounts();
    const palette = this.chartPalette();
    const slices: { x: string; y: number; color: string }[] = [];
    // Mirror React: only include non-zero slices (no invisible wedges).
    if (c.onTrack > 0) slices.push({ x: 'On track', y: c.onTrack, color: palette.onTrack });
    if (c.atRisk > 0) slices.push({ x: 'At risk', y: c.atRisk, color: palette.atRisk });
    if (c.critical > 0) slices.push({ x: 'Critical', y: c.critical, color: palette.critical });
    if (c.notStarted > 0) slices.push({ x: 'Not started', y: c.notStarted, color: palette.notStarted });
    return slices;
  });

  kpiSummary = computed<KpiSummaryItem[] | null>(() => {
    const portfolio = this.portfolio();
    const cost = this.cost();
    if (!portfolio || !cost) return null;
    return [
      { label: 'Active Projects', value: portfolio.activeProjects.toString(), icon: 'arrow-up-right', trend: '3 this quarter', tone: 'positive', to: '/projects?status=Active' },
      {
        label: 'Schedule Variance (SV)',
        value: formatPct(portfolio.scheduleVariancePct),
        icon: portfolio.scheduleVariancePct >= 0 ? 'arrow-up-right' : 'arrow-down-right',
        trend: `${Math.abs(portfolio.scheduleVariancePct).toFixed(1)}% ${portfolio.scheduleVariancePct >= 0 ? 'ahead' : 'behind'} plan`,
        tone: portfolio.scheduleVariancePct >= 0 ? 'positive' : 'negative',
        to: '/projects',
      },
      {
        label: 'Cost Variance (CV)',
        value: formatPct(portfolio.costVariancePct),
        icon: portfolio.costVariancePct >= 0 ? 'arrow-up-right' : 'arrow-down-right',
        trend: portfolio.costVariancePct >= 0 ? 'Under budget' : 'Over budget',
        tone: portfolio.costVariancePct >= 0 ? 'positive' : 'negative',
        to: '/cost-control?coStatus=UnderReview',
      },
      {
        label: 'CPI',
        value: portfolio.cpi.toFixed(2),
        icon: portfolio.cpi >= 1 ? 'arrow-up-right' : 'alert-circle',
        trend: portfolio.cpi >= 1 ? 'On target' : 'Below target',
        tone: portfolio.cpi >= 1 ? 'positive' : 'warning',
        to: '/cost-control?coStatus=UnderReview',
      },
      {
        label: 'SPI',
        value: portfolio.spi.toFixed(2),
        icon: portfolio.spi >= 1 ? 'arrow-up-right' : 'arrow-down-right',
        trend: portfolio.spi >= 1 ? 'Schedule on track' : 'Recovery needed',
        tone: portfolio.spi >= 1 ? 'positive' : 'negative',
        to: '/cost-control?coStatus=UnderReview',
      },
      {
        label: 'Open Risks',
        value: portfolio.openRisks.toString(),
        icon: portfolio.criticalRisks > 0 ? 'alert-triangle' : 'check-circle',
        trend: `${portfolio.criticalRisks} critical`,
        tone: portfolio.criticalRisks > 0 ? 'negative' : 'positive',
        to: '/risks?severity=Critical',
      },
    ];
  });

  constructor(private reports: ReportsService) {}

  ngOnInit(): void {
    this.tokens.init();
    this.loading.set(true);
    this.error.set(null);
    forkJoin([
      this.reports.getPortfolioKpis(),
      this.reports.getCostKpis(),
      this.reports.getProjectHealthDistribution(),
      this.reports.getCostPerformanceTrend(6),
      this.reports.getUpcomingMilestones(14, 10),
    ]).subscribe({
      next: ([portfolio, cost, health, trend, milestones]) => {
        this.portfolio.set(portfolio);
        this.cost.set(cost);
        this.health.set(health);
        this.trend.set(trend);
        this.milestones.set(milestones);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err instanceof Error ? err.message : 'Failed to load dashboard');
        this.loading.set(false);
      },
    });
  }

  goTo(path: string): void {
    this.router.navigateByUrl(path);
  }

  onKpiClick(kpi: KpiSummaryItem): void {
    if (kpi.to) this.goTo(kpi.to);
  }

  onKpiKeydown(event: KeyboardEvent, kpi: KpiSummaryItem): void {
    if (!kpi.to) return;
    onActivateKey(event, () => this.goTo(kpi.to!));
  }
}
