import { Component, Input, OnChanges, TemplateRef, ViewChild, effect, signal } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  PdfViewerModule,
  ToolbarService,
  MagnificationService,
  NavigationService,
  LinkAnnotationService,
  BookmarkViewService,
  ThumbnailViewService,
  PrintService,
  TextSelectionService,
  TextSearchService,
} from '@syncfusion/ej2-angular-pdfviewer';
import { GridModule, ResizeService } from '@syncfusion/ej2-angular-grids';
import { ProjectsService } from '../../core/services/projects.service';
import { Modal } from '../../shared/components/modal/modal';
import { IconComponent } from '../../shared/components/icon';
import { OutletContextService } from '../../layout/outlet-context.service';
import type {
  ChangeOrderSummaryDto,
  HealthStatus,
  ProjectDetailDto,
  ProjectKpisDto,
  ProjectMilestoneDto,
  RecentDocumentDto,
  RfiSummaryDto,
  RiskSummaryDto,
  SubmittalSummaryDto,
  TaskStatus,
} from '../../core/models/api.models';
import { onActivateKey } from '../../shared/utils/a11y';
import { formatDate } from '../../shared/utils/date.util';
import { formatCurrency } from '../../shared/utils/format.util';

// Demo documents don't have real per-file content in the sample data set, so every
// preview opens the same sample PDF (mirrors the approach used on the Documents page).
const SAMPLE_PDF_URL = 'https://cdn.syncfusion.com/content/pdf/pdf-succinctly.pdf';

const healthBadgeClass: Record<HealthStatus, string> = {
  NotStarted: 'badge-neutral',
  OnTrack: 'badge-success',
  AtRisk: 'badge-warning',
  Critical: 'badge-error',
};

const riskAlertClass: Record<string, string> = {
  Critical: 'alert-error',
  High: 'alert-warning',
  Medium: 'alert-info',
  Low: 'alert-info',
};

const milestoneStatusBadgeClass: Record<TaskStatus, string> = {
  NotStarted: 'badge-neutral',
  InProgress: 'badge-info',
  OnHold: 'badge-warning',
  Completed: 'badge-success',
  Cancelled: 'badge-error',
};

const milestoneStatusLabel: Record<TaskStatus, string> = {
  NotStarted: 'Not Started',
  InProgress: 'In Progress',
  OnHold: 'On Hold',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
};

function documentStatusClass(status: string): string {
  const mapped = status.toLowerCase();
  if (['approved', 'answered', 'uploaded'].includes(mapped)) return 'badge-success';
  if (['under review', 'submitted', 'draft'].includes(mapped)) return 'badge-warning';
  if (['rejected'].includes(mapped)) return 'badge-error';
  return 'badge-info';
}

function changeOrderStatusClass(status: string): string {
  const mapped = status.toLowerCase();
  if (mapped === 'approved') return 'badge-success';
  if (['underreview', 'submitted'].includes(mapped)) return 'badge-warning';
  if (mapped === 'rejected') return 'badge-error';
  return 'badge-info';
}

function rfiStatusClass(status: string): string {
  const mapped = status.toLowerCase();
  if (mapped === 'answered' || mapped === 'closed') return 'badge-success';
  if (mapped === 'open' || mapped === 'overdue') return 'badge-warning';
  if (mapped === 'rejected') return 'badge-error';
  return 'badge-info';
}

function submittalStatusClass(status: string): string {
  const mapped = status.toLowerCase();
  if (['approved', 'accepted'].includes(mapped)) return 'badge-success';
  if (['pending', 'submitted', 'under review'].includes(mapped)) return 'badge-warning';
  if (['rejected', 'revise and resubmit'].includes(mapped)) return 'badge-error';
  return 'badge-info';
}

function formatPercent(n: number): string {
  return `${n > 0 ? '+' : ''}${n}%`;
}

type DetailTab = 'Overview' | 'Schedule' | 'Cost' | 'RFIs' | 'Submittals';
const tabs: DetailTab[] = ['Overview', 'Schedule', 'Cost', 'RFIs', 'Submittals'];

@Component({
  selector: 'app-project-detail',
  imports: [Modal, PdfViewerModule, GridModule, IconComponent],
  providers: [
    ToolbarService,
    MagnificationService,
    NavigationService,
    LinkAnnotationService,
    BookmarkViewService,
    ThumbnailViewService,
    PrintService,
    TextSelectionService,
    TextSearchService,
    ResizeService,
  ],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.css',
})
export class ProjectDetail implements OnChanges {
  @Input() id!: string;

  readonly tabs = tabs;
  readonly healthBadgeClass = healthBadgeClass;
  readonly riskAlertClass = riskAlertClass;
  readonly milestoneStatusBadgeClass = milestoneStatusBadgeClass;
  readonly milestoneStatusLabel = milestoneStatusLabel;
  readonly documentStatusClass = documentStatusClass;
  readonly changeOrderStatusClass = changeOrderStatusClass;
  readonly rfiStatusClass = rfiStatusClass;
  readonly submittalStatusClass = submittalStatusClass;
  readonly formatDate = formatDate;
  readonly formatCurrency = formatCurrency;
  readonly formatPercent = formatPercent;
  readonly sampleUrl = SAMPLE_PDF_URL;

  // Template-side lookups (Syncfusion's <ng-template let-m> context is `any`,
  // so `milestoneStatusBadgeClass[m.status]` triggers TS7053 under strict
  // template checking). These wrappers accept a `string` and cast on the TS side.
  healthBadge(s: string): string { return healthBadgeClass[s as HealthStatus] ?? ''; }
  riskAlertClassFor(s: string): string { return riskAlertClass[s] ?? 'alert-info'; }
  milestoneStatusBadge(s: string): string { return milestoneStatusBadgeClass[s as TaskStatus] ?? ''; }
  milestoneStatusLabelFor(s: string): string { return milestoneStatusLabel[s as TaskStatus] ?? s; }

  project = signal<ProjectDetailDto | null>(null);
  kpis = signal<ProjectKpisDto | null>(null);
  milestones = signal<ProjectMilestoneDto[]>([]);
  risks = signal<RiskSummaryDto[]>([]);
  documents = signal<RecentDocumentDto[]>([]);
  rfis = signal<RfiSummaryDto[]>([]);
  submittals = signal<SubmittalSummaryDto[]>([]);
  changeOrders = signal<ChangeOrderSummaryDto[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  activeTab = signal<DetailTab>('Overview');
  previewDocument = signal<RecentDocumentDto | null>(null);

  // Topbar breadcrumb template — rendered into the topbar's left slot by
  // `Shell` via `OutletContextService.setTopbarHeading`. The template is
  // captured by ViewChild on the `<ng-template #topbarBreadcrumb>` block
  // inside project-detail.html so the same template the page renders in
  // its in-content `<header>` is reused in the topbar without duplicating
  // markup.
  @ViewChild('topbarBreadcrumb', { static: false })
  topbarBreadcrumbTpl?: TemplateRef<unknown>;

  constructor(
    private projectsApi: ProjectsService,
    private router: Router,
    private outlet: OutletContextService,
  ) {
    // Whenever the project changes, push the breadcrumb template into the
    // topbar (and clear it when the page unmounts / the project is null).
    effect(() => {
      const project = this.project();
      if (project && this.topbarBreadcrumbTpl) {
        this.outlet.setTopbarHeading(this.topbarBreadcrumbTpl);
      } else {
        this.outlet.setTopbarHeading(null);
      }
    });
  }

  ngOnChanges(): void {
    const projectId = Number(this.id);
    if (Number.isNaN(projectId)) {
      this.error.set('Invalid project ID');
      this.loading.set(false);
      return;
    }
    this.loading.set(true);
    this.error.set(null);
    forkJoin([
      this.projectsApi.getById(projectId),
      this.projectsApi.getKpis(projectId),
      this.projectsApi.getUpcomingMilestones(projectId, 30, 10),
      this.projectsApi.getTopRisks(projectId, 5),
      this.projectsApi.getRecentDocuments(projectId, 30, 10),
      this.projectsApi.getRfis(projectId, 50),
      this.projectsApi.getSubmittals(projectId, 50),
      this.projectsApi.getChangeOrders(projectId, 50),
    ]).subscribe({
      next: ([project, kpis, milestones, risks, documents, rfis, submittals, changeOrders]) => {
        this.project.set(project);
        this.kpis.set(kpis);
        this.milestones.set(milestones);
        this.risks.set(risks);
        this.documents.set(documents);
        this.rfis.set(rfis);
        this.submittals.set(submittals);
        this.changeOrders.set(changeOrders);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err instanceof Error ? err.message : 'Failed to load project details');
        this.loading.set(false);
      },
    });
  }

  get headerSubtitle(): string {
    const project = this.project();
    if (!project) return '';
    const parts = [
      project.location,
      project.startDate && project.endDate ? `${formatDate(project.startDate)} – ${formatDate(project.endDate)}` : null,
      project.budget != null ? `${formatCurrency(project.budget)} budget` : null,
    ].filter(Boolean);
    return parts.join(' · ');
  }

  tabBadge(tab: DetailTab): number {
    const kpis = this.kpis();
    if (!kpis) return 0;
    if (tab === 'RFIs') return kpis.openRfis;
    if (tab === 'Submittals') return kpis.openSubmittals;
    return 0;
  }

  splitHealthLabel(status: HealthStatus): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  goBack(): void {
    this.router.navigate(['/projects']);
  }

  goTo(path: string): void {
    this.router.navigate([path]);
  }

  onOpenRfisKeydown(event: KeyboardEvent): void {
    onActivateKey(event, () => this.goTo('/rfis'));
  }

  onDocumentRowKeydown(event: KeyboardEvent, doc: RecentDocumentDto): void {
    onActivateKey(event, () => this.previewDocument.set(doc));
  }

  onDocumentRowSelected(args: { data?: unknown }): void {
    const doc = args.data as RecentDocumentDto | undefined;
    if (doc) this.previewDocument.set(doc);
  }

  spentPercent(spent: number, budget: number): number {
    if (!budget) return 0;
    return Math.min(100, Math.round((spent / budget) * 100));
  }
}
