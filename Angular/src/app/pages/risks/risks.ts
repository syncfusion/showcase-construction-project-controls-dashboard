import { Component, OnInit, signal, computed, effect, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { RisksService } from '../../core/services/risks.service';
import { RiskMatrixService } from '../../core/services/risk-matrix.service';
import type { RiskKpisDto, RiskMatrixCellViewModel, RiskProbability, RiskSeverity, RiskStatus, RiskSummaryDto } from '../../core/models/api.models';
import { Modal } from '../../shared/components/modal/modal';
import { IconComponent } from '../../shared/components/icon';
import { RiskMatrixHeatmapComponent } from '../../shared/components/risk-matrix-heatmap/risk-matrix-heatmap.component';
import { onActivateKey } from '../../shared/utils/a11y';
import { downloadCsv } from '../../shared/utils/csv';
import { formatDate } from '../../shared/utils/date.util';
import { GridModule, PageService, SortService, ResizeService } from '@syncfusion/ej2-angular-grids';
import type { RowSelectEventArgs } from '@syncfusion/ej2-grids';

interface NewRiskDraft {
  title: string;
  description: string;
  projectId: string;
  projectCode: string;
  severity: RiskSeverity;
  probability: RiskProbability;
  owner: string;
  mitigationPlan: string;
  targetResolutionDate: string;
  impactDays: string;
  impactCost: string;
}

function emptyRiskDraft(): NewRiskDraft {
  return {
    title: '', description: '', projectId: '', projectCode: '', severity: 'Medium', probability: 'Medium',
    owner: '', mitigationPlan: '', targetResolutionDate: '', impactDays: '', impactCost: '',
  };
}

const severityBadgeClass: Record<RiskSeverity, string> = { Critical: 'badge-error', High: 'badge-warning', Medium: 'badge-info', Low: 'badge-neutral' };
const statusBadgeClass: Record<RiskStatus, string> = {
  Open: 'badge-error', InProgress: 'badge-warning', Monitoring: 'badge-info', Escalated: 'badge-error',
  Containment: 'badge-warning', Mitigated: 'badge-success', Closed: 'badge-neutral',
};

type KpiKey = 'critical' | 'high' | 'medium' | 'mitigated';
const kpiBorderColor: Record<KpiKey, string> = { critical: 'var(--color-error)', high: 'var(--color-warning)', medium: 'var(--color-warning)', mitigated: 'var(--color-success)' };
const kpiIcon: Record<KpiKey, string> = { critical: 'shield-alert', high: 'alert-triangle', medium: 'activity', mitigated: 'check-circle' };
const kpiChangeTone: Record<KpiKey, string> = { critical: 'negative', high: 'warning', medium: 'text-secondary', mitigated: 'positive' };
const kpiChangeLabel: Record<KpiKey, string> = { critical: 'Immediate action required', high: 'Watch closely', medium: 'Monitored', mitigated: 'On track' };

const severityOptions: (RiskSeverity | 'All')[] = ['All', 'Critical', 'High', 'Medium', 'Low'];
const statusOptions: (RiskStatus | 'All')[] = ['All', 'Open', 'InProgress', 'Monitoring', 'Escalated', 'Containment', 'Mitigated', 'Closed'];
const severities: RiskSeverity[] = ['Low', 'Medium', 'High', 'Critical'];
const probabilities: RiskProbability[] = ['Low', 'Medium', 'High'];

function formatImpact(risk: Pick<RiskSummaryDto, 'impactDays' | 'impactCost'>): string {
  const parts: string[] = [];
  if (risk.impactDays) parts.push(`${risk.impactDays}d`);
  if (risk.impactCost) parts.push(`$${(risk.impactCost / 1_000_000).toFixed(1)}M`);
  if (!parts.length) return 'Minor';
  return parts.join(' · ');
}

@Component({
  selector: 'app-risks',
  imports: [GridModule, Modal, IconComponent, RiskMatrixHeatmapComponent],
  providers: [PageService, SortService, ResizeService],
  templateUrl: './risks.html',
  styleUrl: './risks.css',
})
export class Risks implements OnInit {
  readonly severityBadgeClass = severityBadgeClass;
  readonly statusBadgeClass = statusBadgeClass;
  readonly kpiBorderColor = kpiBorderColor;
  readonly kpiIcon = kpiIcon;
  readonly kpiChangeTone = kpiChangeTone;
  readonly kpiChangeLabel = kpiChangeLabel;
  readonly severityOptions = severityOptions;
  readonly statusOptions = statusOptions;
  readonly severities = severities;
  readonly probabilities = probabilities;
  readonly formatImpact = formatImpact;
  readonly formatDate = formatDate;
  readonly onActivateKey = onActivateKey;

  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // Template-side lookups (Syncfusion's <ng-template let-r> context is `any`,
  // so `severityBadgeClass[r.severity]` triggers TS7053 under strict template
  // checking). These wrappers accept a `string` and cast on the TS side.
  severityBadge(s: string): string { return severityBadgeClass[s as RiskSeverity] ?? ''; }
  statusBadge(s: string): string { return statusBadgeClass[s as RiskStatus] ?? ''; }
  kpiBorderColorFor(s: KpiKey): string { return kpiBorderColor[s] ?? 'var(--color-border)'; }
  kpiChangeToneFor(s: KpiKey): string { return kpiChangeTone[s] ?? 'text-secondary'; }
  kpiIconFor(s: KpiKey): string { return kpiIcon[s] ?? 'activity'; }
  kpiChangeLabelFor(s: KpiKey): string { return kpiChangeLabel[s] ?? ''; }

  risks = signal<RiskSummaryDto[]>([]);
  kpis = signal<RiskKpisDto | null>(null);
  matrix = signal<RiskMatrixCellViewModel[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);

  search = signal('');
  severity = signal<RiskSeverity | 'All'>('All');
  status = signal<RiskStatus | 'All'>('All');

  selectedRisk = signal<RiskSummaryDto | null>(null);
  // HeatMap cell drill — fires from <app-risk-matrix-heatmap (cellClick)>.
  matrixCellRisks = signal<RiskSummaryDto[]>([]);
  matrixCellLabel = signal<string>('');
  showNewRiskModal = signal(false);
  newRiskDraft = signal<NewRiskDraft>(emptyRiskDraft());

  filteredRisks = computed(() => {
    const q = this.search().trim().toLowerCase();
    const severity = this.severity();
    const status = this.status();
    return this.risks().filter((r) => {
      const matchesSearch = !q || r.title.toLowerCase().includes(q) || r.number.toLowerCase().includes(q) || r.projectCode.toLowerCase().includes(q);
      const matchesSeverity = severity === 'All' || r.severity === severity;
      const matchesStatus = status === 'All' || r.status === status;
      return matchesSearch && matchesSeverity && matchesStatus;
    });
  });

  kpiSummary = computed(() => {
    const kpis = this.kpis();
    if (!kpis) return null;
    return [
      { key: 'critical' as const, label: 'Critical', value: kpis.critical },
      { key: 'high' as const, label: 'High', value: kpis.high },
      { key: 'medium' as const, label: 'Medium', value: kpis.medium },
      { key: 'mitigated' as const, label: 'Mitigated this month', value: kpis.mitigatedThisMonth },
    ];
  });

  constructor(private risksApi: RisksService, private riskMatrixApi: RiskMatrixService) {
    // Read ?severity and ?status from URL, mirror React's URLSourceOfTruth pattern.
    const sev = this.route.snapshot.queryParamMap.get('severity');
    const sta = this.route.snapshot.queryParamMap.get('status');
    if (sev && (severityOptions as string[]).includes(sev)) this.severity.set(sev as RiskSeverity | 'All');
    if (sta && (statusOptions as string[]).includes(sta)) this.status.set(sta as RiskStatus | 'All');

    // Push filter state to URL whenever the user picks a different filter
    // (or a KPI card fires `applyKpiFilter`). Mirrors React's `setSearchParams`.
    effect(() => {
      const sev = this.severity();
      const sta = this.status();
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {
          severity: sev === 'All' ? null : sev,
          status: sta === 'All' ? null : sta,
        },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    });
  }

  ngOnInit(): void {
    forkJoin([this.risksApi.getRisks({ page: 1, pageSize: 1000 }), this.risksApi.getKpis(), this.riskMatrixApi.getMatrix()]).subscribe({
      next: ([risksResp, kpis, matrix]) => {
        this.risks.set(risksResp.data);
        this.kpis.set(kpis);
        this.matrix.set(matrix);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err instanceof Error ? err.message : 'Failed to load risks');
        this.loading.set(false);
      },
    });
  }

  // HeatMap cell drill — pop a modal listing all risks in this (prob × sev)
  // bucket. Mirrors React's `handleMatrixCellClick`.
  handleMatrixCellClick(ev: { probability: RiskProbability; severity: RiskSeverity }): void {
    const cell = this.matrix().find((c) => c.probability === ev.probability && c.severity === ev.severity);
    if (!cell || cell.riskIds.length === 0) return;
    const cellRisks = cell.riskIds
      .map((num) => this.risks().find((r) => r.number === num))
      .filter((r): r is RiskSummaryDto => r !== undefined);
    if (cellRisks.length === 0) return;
    this.matrixCellLabel.set(`${ev.probability} Probability · ${ev.severity} Impact`);
    this.matrixCellRisks.set(cellRisks);
  }

  severityBorderColor(sev: RiskSeverity): string {
    if (sev === 'Critical' || sev === 'High') return 'var(--color-error)';
    if (sev === 'Medium') return 'var(--color-warning)';
    return 'var(--color-success)';
  }

  onSearchChange(value: string): void {
    this.search.set(value);
  }

  onSeverityChange(value: RiskSeverity | 'All'): void {
    this.severity.set(value);
  }

  onStatusChange(value: RiskStatus | 'All'): void {
    this.status.set(value);
  }

  applyKpiFilter(key: KpiKey): void {
    if (key === 'mitigated') {
      this.status.set('Mitigated');
      this.severity.set('All');
      return;
    }
    const severityByKpi: Record<'critical' | 'high' | 'medium', RiskSeverity> = { critical: 'Critical', high: 'High', medium: 'Medium' };
    this.severity.set(severityByKpi[key]);
    this.status.set('All');
  }

  onKpiKeydown(event: KeyboardEvent, key: KpiKey): void {
    onActivateKey(event, () => this.applyKpiFilter(key));
  }

  onRowSelected(args: RowSelectEventArgs): void {
    const data = args.data as RiskSummaryDto | undefined;
    if (data) this.selectedRisk.set(data);
  }

  viewProjectDetails(): void {
    const risk = this.selectedRisk();
    if (!risk) return;
    const projectId = risk.projectId;
    this.selectedRisk.set(null);
    this.router.navigate(['/projects', projectId]);
  }

  // Used by the HeatMap cell-drill modal — drill from a cell's risk row to the
  // risk detail modal, and the "View Project" button to jump to the project.
  // Both flows are also bound to keyboard activation; we route them through
  // `onActivateKey` for a single keyboard-handling policy.
  openRiskFromCellDrill(r: RiskSummaryDto): void {
    this.matrixCellRisks.set([]);
    this.selectedRisk.set(r);
  }

  onCellDrillKeydown(event: KeyboardEvent, r: RiskSummaryDto): void {
    onActivateKey(event, () => this.openRiskFromCellDrill(r));
  }

  viewProjectForRisk(r: RiskSummaryDto): void {
    this.matrixCellRisks.set([]);
    this.router.navigate(['/projects', r.projectId]);
  }

  onViewProjectKeydown(event: KeyboardEvent, r: RiskSummaryDto): void {
    onActivateKey(event, () => this.viewProjectForRisk(r));
  }

  openNewRiskModal(): void {
    this.newRiskDraft.set(emptyRiskDraft());
    this.showNewRiskModal.set(true);
  }

  updateDraft(patch: Partial<NewRiskDraft>): void {
    this.newRiskDraft.update((d) => ({ ...d, ...patch }));
  }

  handleSaveNewRisk(): void {
    const draft = this.newRiskDraft();
    if (!draft.title.trim()) return;
    const risks = this.risks();
    const nextId = risks.length ? Math.max(...risks.map((r) => r.id)) + 1 : 1;
    const impactDays = draft.impactDays ? Number(draft.impactDays) : undefined;
    const impactCost = draft.impactCost ? Number(draft.impactCost) : undefined;
    const created: RiskSummaryDto = {
      id: nextId,
      projectId: Number(draft.projectId) || 0,
      projectCode: draft.projectCode.trim() || 'TBD',
      number: `RISK-${String(nextId).padStart(4, '0')}`,
      title: draft.title.trim(),
      description: draft.description.trim() || undefined,
      severity: draft.severity,
      probability: draft.probability,
      impactCost,
      impactDays,
      owner: draft.owner.trim() || undefined,
      status: 'Open',
      mitigationPlan: draft.mitigationPlan.trim() || undefined,
      identifiedDate: new Date().toISOString(),
      targetResolutionDate: draft.targetResolutionDate || undefined,
      impactDisplay: '',
    };
    created.impactDisplay = formatImpact(created);
    // Demo only: kept in local component state so it's visible in the UI immediately;
    // nothing is written back to the API.
    this.risks.set([created, ...risks]);
    this.severity.set('All');
    this.status.set('All');
    this.search.set('');
    this.showNewRiskModal.set(false);
  }

  handleExportRisks(): void {
    downloadCsv<RiskSummaryDto>(
      'risks',
      [
        { header: 'ID', value: (r) => r.number },
        { header: 'Risk / Issue', value: (r) => r.title },
        { header: 'Project', value: (r) => r.projectCode },
        { header: 'Severity', value: (r) => r.severity },
        { header: 'Probability', value: (r) => r.probability },
        { header: 'Impact', value: (r) => formatImpact(r) },
        { header: 'Owner', value: (r) => r.owner ?? '' },
        { header: 'Status', value: (r) => r.status },
      ],
      this.filteredRisks(),
    );
  }
}
