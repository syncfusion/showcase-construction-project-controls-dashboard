import type { ReactElement } from 'react';
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { GridComponent, ColumnsDirective, ColumnDirective, Inject, Resize } from '@syncfusion/ej2-react-grids';
import { Icon } from '../components/Icon';
import {
  AccumulationChartComponent,
  AccumulationSeriesCollectionDirective,
  AccumulationSeriesDirective,
  Category,
  ChartComponent,
  ColumnSeries,
  Inject as ChartInject,
  PieSeries,
  SeriesCollectionDirective,
  SeriesDirective,
  Tooltip,
} from '@syncfusion/ej2-react-charts';
import { reportsApi } from '../api/reports';
import type { CostKpisDto, CostPerformancePointDto, HealthStatus, PortfolioKpisDto, ProjectHealthDistributionDto, UpcomingMilestoneDto } from '../types';
import { onActivateKey } from '../utils/a11y';
import { format } from '../utils/date';
import { formatCompactCurrency } from '../utils/format';
import { getThemeScopeElement } from '../utils/theme';
import './DashboardPage.css';

const healthClasses: Record<HealthStatus, string> = {
  NotStarted: 'text-secondary',
  OnTrack: 'positive',
  AtRisk: 'warning',
  Critical: 'negative',
};

function formatPct(n?: number | null): string {
  if (n === undefined || n === null) return '—';
  return `${n >= 0 ? '+' : ''}${n.toFixed(1)}%`;
}

/**
 * Resolve a CSS custom property on the theme-scoped element (`.app-shell`,
 * where `data-theme` actually lives — see getThemeScopeElement) to a
 * literal color string. Syncfusion charts take their palette as literal
 * hex/rgba values (not CSS vars) because the values land inside SVG `fill`
 * attributes, so we look the value up and pass it through.
 */
function resolveTokenColor(name: string, fallback: string): string {
  if (typeof window === 'undefined') return fallback;
  const value = getComputedStyle(getThemeScopeElement()).getPropertyValue(name).trim();
  return value || fallback;
}

/**
 * Theme-bridge hook that bumps a counter whenever the active theme changes,
 * so any token-driven chart re-resolves its palette on toggle. Mirrors the
 * MutationObserver pattern used by RiskMatrixHeatmap.tsx.
 */
function useThemeRevision(): number {
  const [revision, setRevision] = useState(0);
  useEffect(() => {
    const bump = () => setRevision((n) => n + 1);
    // The token resolution driven by this counter runs via useMemo during
    // the render phase, so the very first pass (revision === 0) happens
    // before this component's own tree — including `.app-shell` — has
    // actually been committed to the DOM, and getThemeScopeElement() falls
    // back to `document.documentElement` for that one pass. Bumping once
    // here (effects run after commit) forces a second, correct pass now
    // that `.app-shell` genuinely exists.
    bump();
    const observer = new MutationObserver(bump);
    observer.observe(getThemeScopeElement(), { attributes: true, attributeFilter: ['class', 'data-theme'] });
    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    mq.addEventListener('change', bump);
    return () => {
      observer.disconnect();
      mq.removeEventListener('change', bump);
    };
  }, []);
  return revision;
}

/**
 * Tracks the rendered pixel width of a referenced element so we can hand
 * ej2 charts an explicit `width={npx}` instead of relying on the chart's
 * built-in `width="100%"` measurement. The ej2 React ChartComponent measures
 * `availableSize` once at mount; if the parent layout hadn't yet settled
 * (or shrinks on a later browser resize), the SVG renders at a stale width
 * and gets cropped.
 *
 * Uses a callback ref (rather than `useRef`) so the ResizeObserver attaches
 * the moment the element actually mounts — important here because the chart
 * wrapper doesn't enter the DOM until after the API resolves (`loading`
 * flips from true → false), which is well after a vanilla `useEffect([])`
 * hook first ran with a `null` ref.
 *
 * Widths are quantised to the nearest 8px so a tiny sub-pixel reflow during
 * a drag doesn't force the consumer to re-mount (consumers use the width as
 * a React `key`, which is the only reliable way to make ej2 actually re-
 * render the SVG to the new size — `refresh()`/prop changes alone don't).
 */
function useElementWidth<T extends HTMLElement>(): [(node: T | null) => void, number] {
  const observerRef = useRef<ResizeObserver | null>(null);
  const [width, setWidth] = useState(0);
  const setNode = useCallback((node: T | null) => {
    // Detach any observer bound to the previous element.
    if (observerRef.current) {
      observerRef.current.disconnect();
      observerRef.current = null;
    }
    if (!node) return;
    const update = () => setWidth(Math.round(node.clientWidth / 8) * 8);
    update();
    const ro = new ResizeObserver(() => update());
    ro.observe(node);
    observerRef.current = ro;
  }, []);
  return [setNode, width];
}

interface ChartTokens {
  axisLabel: string;
  axisLine: string;
  gridline: string;
  tooltipBg: string;
  tooltipText: string;
  planned: string;
  actual: string;
  onTrack: string;
  atRisk: string;
  critical: string;
  notStarted: string;
  foreground: string;
}

function resolveChartTokens(): ChartTokens {
  return {
    axisLabel: resolveTokenColor('--color-secondary', '#475467'),
    axisLine: resolveTokenColor('--color-border', '#eaecf0'),
    gridline: resolveTokenColor('--color-border-subtle', '#f2f4f7'),
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
}

export function DashboardPage(): ReactElement {
  const navigate = useNavigate();
  const [portfolio, setPortfolio] = useState<PortfolioKpisDto | null>(null);
  const [cost, setCost] = useState<CostKpisDto | null>(null);
  const [health, setHealth] = useState<ProjectHealthDistributionDto | null>(null);
  const [trend, setTrend] = useState<CostPerformancePointDto[]>([]);
  const [milestones, setMilestones] = useState<UpcomingMilestoneDto[]>([]);
  const [loading, setLoading] = useState(true);
  const milestonesGridRef = useRef<GridComponent>(null);
  const [error, setError] = useState<string | null>(null);
  const themeRevision = useThemeRevision();
  // Measured pixel width of the cost-trend chart wrapper. ej2 measures the
  // chart's availableSize once at mount; handing it an explicit pixel width
  // (kept in sync with the wrapper via a ResizeObserver) makes the chart
  // follow the card width on every browser resize instead of clipping.
  const [trendContainerRef, trendWidth] = useElementWidth<HTMLDivElement>();

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    Promise.all([
      reportsApi.getPortfolioKpis(),
      reportsApi.getCostKpis(),
      reportsApi.getProjectHealthDistribution(),
      reportsApi.getCostPerformanceTrend(6),
      reportsApi.getUpcomingMilestones(14, 10),
    ])
      .then(([portfolioData, costData, healthData, trendData, milestoneData]) => {
        if (cancelled) return;
        setPortfolio(portfolioData);
        setCost(costData);
        setHealth(healthData);
        setTrend(trendData);
        setMilestones(milestoneData);
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Failed to load dashboard');
        setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const [onTrack, atRisk, critical, notStarted, total] = useMemo(
    () => (health ? [health.onTrack, health.atRisk, health.critical, health.notStarted, health.onTrack + health.atRisk + health.critical + health.notStarted] : [0, 0, 0, 0, 0]),
    [health]
  );

  // Token values resolved at render time so light/dark theme switches
  // (which mutate --color-* variables on :root via data-theme / class) are
  // reflected in the chart palette on the next render.
  const chartTokens = useMemo(resolveChartTokens, [themeRevision]);

  // Syncfusion ColumnSeries data — typed as {x, planned, actual}.
  const costTrendData = useMemo(
    () => trend.map((point) => ({ x: point.month, planned: point.planned, actual: point.actual })),
    [trend],
  );

  // Syncfusion PieSeries data — only include non-zero slices so the doughnut
  // doesn't render an invisible "Not started" wedge that the legend still
  // surfaces. The full count is shown in the center label. Each slice carries
  // its own `color` so we can drive slice colors from project tokens via the
  // series `pointColorMapping` (AccumulationChart has no `palettes` prop).
  const healthDonutData = useMemo(
    () => {
      const slices: { x: string; y: number; color: string }[] = [];
      if (onTrack > 0) slices.push({ x: 'On track', y: onTrack, color: chartTokens.onTrack });
      if (atRisk > 0) slices.push({ x: 'At risk', y: atRisk, color: chartTokens.atRisk });
      if (critical > 0) slices.push({ x: 'Critical', y: critical, color: chartTokens.critical });
      if (notStarted > 0) slices.push({ x: 'Not started', y: notStarted, color: chartTokens.notStarted });
      return slices;
    },
    [onTrack, atRisk, critical, notStarted, chartTokens],
  );

  const kpiSummary = useMemo(() => {
    if (!portfolio || !cost) return null;
    return [
      { label: 'Active Projects', value: portfolio.activeProjects.toString(), icon: 'arrow-up-right', trend: '3 this quarter', tone: 'positive' as const, to: '/projects?status=Active' },
      { label: 'Schedule Variance (SV)', value: formatPct(portfolio.scheduleVariancePct), icon: portfolio.scheduleVariancePct >= 0 ? 'arrow-up-right' : 'arrow-down-right', trend: `${Math.abs(portfolio.scheduleVariancePct).toFixed(1)}% ${portfolio.scheduleVariancePct >= 0 ? 'ahead' : 'behind'} plan`, tone: portfolio.scheduleVariancePct >= 0 ? 'positive' : 'negative' as const, to: '/projects' },
      { label: 'Cost Variance (CV)', value: formatPct(portfolio.costVariancePct), icon: portfolio.costVariancePct >= 0 ? 'arrow-up-right' : 'arrow-down-right', trend: portfolio.costVariancePct >= 0 ? 'Under budget' : 'Over budget', tone: portfolio.costVariancePct >= 0 ? 'positive' : 'negative' as const, to: '/cost-control?coStatus=UnderReview' },
      { label: 'CPI', value: portfolio.cpi.toFixed(2), icon: portfolio.cpi >= 1 ? 'arrow-up-right' : 'alert-circle', trend: portfolio.cpi >= 1 ? 'On target' : 'Below target', tone: portfolio.cpi >= 1 ? 'positive' : 'warning' as const, to: '/cost-control?coStatus=UnderReview' },
      { label: 'SPI', value: portfolio.spi.toFixed(2), icon: portfolio.spi >= 1 ? 'arrow-up-right' : 'arrow-down-right', trend: portfolio.spi >= 1 ? 'Schedule on track' : 'Recovery needed', tone: portfolio.spi >= 1 ? 'positive' : 'negative' as const, to: '/cost-control?coStatus=UnderReview' },
      { label: 'Open Risks', value: portfolio.openRisks.toString(), icon: portfolio.criticalRisks > 0 ? 'alert-triangle' : 'check-circle', trend: `${portfolio.criticalRisks} critical`, tone: portfolio.criticalRisks > 0 ? 'negative' : 'positive' as const, to: '/risks?severity=Critical' },
    ];
  }, [portfolio]);

  return (
    <div className="dashboard-page">
      {loading && <div className="loading-state" aria-live="polite">Loading dashboard…</div>}
      {error && <div className="alert alert-error" role="alert">{error}</div>}

      {!loading && !error && portfolio && cost && health && (
        <>
          <div className="kpi-grid">
            {kpiSummary?.map((kpi) => (
              <div
                className={`kpi-card${kpi.to ? ' is-clickable' : ''}`}
                key={kpi.label}
                {...(kpi.to
                  ? {
                      role: 'button' as const,
                      tabIndex: 0,
                      'aria-label': `View ${kpi.label}`,
                      onClick: () => navigate(kpi.to!),
                      onKeyDown: onActivateKey(() => navigate(kpi.to!)),
                    }
                  : {})}
              >
                <div className="kpi-label">{kpi.label}</div>
                <div className="kpi-value">{kpi.value}</div>
                <div className={`kpi-change ${kpi.tone}`}>
                  <Icon className="icon" name={kpi.icon} size={14} />
                  {kpi.trend}
                </div>
              </div>
            ))}
          </div>

          <div className="two-column-grid" style={{ marginTop: 'var(--space-xl)' }}>
            <div className="card">
              <div className="card-header">
                <div>
                  <h2 className="card-title">Cost Performance Trend</h2>
                  <p className="card-subtitle">Earned value vs. actual spend</p>
                </div>
                <button type="button" className="btn btn-secondary btn-sm" onClick={() => navigate('/reports')}>
                  View report
                </button>
              </div>
              <div
                className="dashboard-chart"
                ref={trendContainerRef}
                data-trend-width={trendWidth}
                role="img"
                aria-label="Cost performance trend, planned versus actual by month"
              >
                {costTrendData.length === 0 ? (
                  <div className="empty-state" style={{ width: '100%' }}>
                    <Icon className="icon" name="bar-chart-3" size={48} />
                    <p>No trend data available</p>
                  </div>
                ) : (
                  <ChartComponent
                    id="dashboard-cost-trend"
                    className="dashboard-cost-trend"
                    key={trendWidth > 0 ? `cost-trend-${trendWidth}` : 'cost-trend-init'}
                    width={trendWidth > 0 ? `${trendWidth}px` : '100%'}
                    height="208px"
                    background="transparent"
                    enableAutoIntervalOnBothAxis={false}
                    border={{ width: 0, color: 'transparent' }}
                    chartArea={{ border: { width: 0, color: 'transparent' } }}
                    primaryXAxis={{
                      valueType: 'Category',
                      interval: 1,
                      majorGridLines: { width: 0 },
                      majorTickLines: { width: 0 },
                      lineStyle: { color: chartTokens.axisLine, width: 1 },
                      labelStyle: {
                        color: chartTokens.axisLabel,
                        fontFamily: 'var(--font-sans)',
                        size: 'var(--text-caption-size)',
                        fontWeight: '500',
                      },
                    }}
                    primaryYAxis={{
                      visible: false,
                      majorGridLines: { width: 0 },
                      lineStyle: { width: 0 },
                      majorTickLines: { width: 0 },
                    }}
                    tooltip={{
                      enable: true,
                      shared: true,
                      fill: chartTokens.tooltipBg,
                      border: { width: 0 },
                      textStyle: {
                        color: chartTokens.tooltipText,
                        fontFamily: 'var(--font-sans)',
                        size: '12px',
                      },
                      format: '${series.name}: ${point.y}',
                    }}
                    
                    sharedTooltipRender={(args: { text?: string[]; headerText?: string }) => {
                      const rows = Array.isArray(args.text) ? args.text : [];
                      // Each row string is originally "SeriesName: <number>" — strip
                      // the trailing number, then reformat with compact currency.
                      const next: string[] = [];
                      rows.forEach((row) => {
                        const nameMatch = row.match(/^([^:]+):/);
                        const seriesName = nameMatch ? nameMatch[1].trim() : row;
                        const valueMatch = row.match(/([\d.,\-]+)/g);
                        const numericStr = valueMatch && valueMatch.length > 0 ? valueMatch[valueMatch.length - 1].replace(/,/g, '') : '';
                        const numeric = Number.parseFloat(numericStr);
                        const value = Number.isFinite(numeric) ? numeric : 0;
                        // "Planned  $5.32M" / "Actual  $5.04M"
                        next.push(`${seriesName}  ${formatCompactCurrency(value)}`);
                      });
                      if (next.length > 0) args.text = next;
                    }}
                    legendSettings={{ visible: false }}
                    margin={{ top: 6, right: 4, bottom: 2, left: 4 }}
                    palettes={[chartTokens.planned, chartTokens.actual]}
                  >
                    <ChartInject services={[ColumnSeries, Category, Tooltip]} />
                    <SeriesCollectionDirective>
                      <SeriesDirective
                        dataSource={costTrendData}
                        xName="x"
                        yName="planned"
                        name="Planned"
                        type="Column"
                        columnWidth={0.4}
                        columnSpacing={0.06}
                        cornerRadius={{ topLeft: 2, topRight: 2 }}
                      />
                      <SeriesDirective
                        dataSource={costTrendData}
                        xName="x"
                        yName="actual"
                        name="Actual"
                        type="Column"
                        columnWidth={0.4}
                        columnSpacing={0.06}
                        cornerRadius={{ topLeft: 2, topRight: 2 }}
                      />
                    </SeriesCollectionDirective>
                  </ChartComponent>
                )}
              </div>
              <div className="chart-legend">
                <span><span className="legend-swatch" style={{ background: chartTokens.planned }} aria-hidden="true" />Planned</span>
                <span><span className="legend-swatch" style={{ background: chartTokens.actual }} aria-hidden="true" />Actual</span>
              </div>
            </div>

            <div className="card">
              <div className="card-header">
                <div>
                  <h2 className="card-title">Projects by Health</h2>
                  <p className="card-subtitle">Risk-weighted health distribution</p>
                </div>
              </div>
              <div className="health-chart">
                <div className="health-chart-donut" role="img" aria-label={`Project health distribution: ${onTrack} on track, ${atRisk} at risk, ${critical} critical, ${notStarted} not started`}>
                  {healthDonutData.length === 0 ? (
                    <div className="empty-state">
                      <p>No projects yet</p>
                    </div>
                  ) : (
                    <>
                      <AccumulationChartComponent
                        id="dashboard-health-donut"
                        className="dashboard-health-donut"
                        width="140px"
                        height="140px"
                        background="transparent"
                        center={{ x: '50%', y: '50%' }}
                        legendSettings={{ visible: false }}
                        enableSmartLabels={false}
                        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
                      >
                        <ChartInject services={[PieSeries]} />
                        <AccumulationSeriesCollectionDirective>
                          <AccumulationSeriesDirective
                            dataSource={healthDonutData}
                            xName="x"
                            yName="y"
                            pointColorMapping="color"
                            innerRadius="56%"
                            radius="100%"
                            startAngle={0}
                            endAngle={360}
                            dataLabel={{ visible: false }}
                          />
                        </AccumulationSeriesCollectionDirective>
                      </AccumulationChartComponent>
                      <div className="donut-hole">
                        <span>{total}</span>
                        <small>Projects</small>
                      </div>
                    </>
                  )}
                </div>
                <div className="health-legend">
                  <div className={`kpi-change ${healthClasses.OnTrack}`}><span className="status-dot success" aria-hidden="true" /> {onTrack} On track</div>
                  <div className={`kpi-change ${healthClasses.AtRisk}`}><span className="status-dot warning" aria-hidden="true" /> {atRisk} At risk</div>
                  <div className={`kpi-change ${healthClasses.Critical}`}><span className="status-dot error" aria-hidden="true" /> {critical} Critical</div>
                  <div className={`kpi-change ${healthClasses.NotStarted}`}><span className="status-dot" aria-hidden="true" /> {notStarted} Not started</div>
                </div>
              </div>
            </div>
          </div>

          <div className="card grid-card" style={{ marginTop: 'var(--space-xl)' }}>
            <div className="card-header">
              <div>
                <h2 className="card-title">Schedule at a Glance</h2>
                <p className="card-subtitle">Next 14 days across the portfolio</p>
              </div>
            </div>
            <GridComponent
              ref={milestonesGridRef}
              dataSource={milestones}
              allowResizing={true}
              width="100%"
              gridLines="Horizontal"
            >
              <ColumnsDirective>
                <ColumnDirective
                  field="projectCode"
                  headerText="Project"
                  width="140"
                  template={(m: UpcomingMilestoneDto) => (
                    <span className="font-mono">{m.projectCode}</span>
                  )}
                />
                <ColumnDirective
                  field="title"
                  headerText="Milestone"
                  template={(m: UpcomingMilestoneDto) => (
                    <span style={{ whiteSpace: 'normal', wordBreak: 'break-word', display: 'block' }}>
                      {m.title}
                    </span>
                  )}
                />
                <ColumnDirective
                  field="dueDate"
                  headerText="Due Date"
                  width="130"
                  template={(m: UpcomingMilestoneDto) => <span>{format(m.dueDate)}</span>}
                />
                <ColumnDirective
                  field="owner"
                  headerText="Owner"
                  width="180"
                  template={(m: UpcomingMilestoneDto) => <span>{m.owner ?? '—'}</span>}
                />
                <ColumnDirective
                  field="healthStatus"
                  headerText="Status"
                  width="130"
                  template={(m: UpcomingMilestoneDto) => (
                    <span className={`badge ${healthBadgeClass[m.healthStatus]}`}>
                      {m.healthStatus.replace(/([A-Z])/g, ' $1').trim()}
                    </span>
                  )}
                />
              </ColumnsDirective>
              <Inject services={[Resize]} />
            </GridComponent>
          </div>
        </>
      )}
    </div>
  );
}

const healthBadgeClass: Record<HealthStatus, string> = {
  NotStarted: 'badge-neutral',
  OnTrack: 'badge-success',
  AtRisk: 'badge-warning',
  Critical: 'badge-error',
};
