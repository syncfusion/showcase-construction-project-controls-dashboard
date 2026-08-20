import { Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectsService } from '../../core/services/projects.service';
import type { ProjectStatus, ProjectSummaryDto } from '../../core/models/api.models';
import { Modal } from '../../shared/components/modal/modal';
import { IconComponent } from '../../shared/components/icon';
import { onActivateKey } from '../../shared/utils/a11y';
import { downloadCsv } from '../../shared/utils/csv';
import { formatDate } from '../../shared/utils/date.util';
import { formatCurrency } from '../../shared/utils/format.util';
import { GridModule, PageService, SortService, ResizeService } from '@syncfusion/ej2-angular-grids';
import type { RowSelectEventArgs } from '@syncfusion/ej2-grids';

const statusOptions: (ProjectStatus | 'All')[] = ['All', 'Active', 'Planning', 'OnHold', 'Completed', 'Cancelled'];
const newProjectStatusOptions: ProjectStatus[] = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'];

const statusBadgeClass: Record<ProjectStatus, string> = {
  Active: 'badge-success',
  Planning: 'badge-info',
  OnHold: 'badge-warning',
  Completed: 'badge-info',
  Cancelled: 'badge-neutral',
};

interface NewProjectDraft {
  name: string;
  code: string;
  location: string;
  startDate: string;
  endDate: string;
  budget: string;
  manager: string;
  status: ProjectStatus;
  description: string;
}

function emptyProjectDraft(): NewProjectDraft {
  const today = new Date().toISOString().slice(0, 10);
  return { name: '', code: '', location: '', startDate: today, endDate: today, budget: '', manager: '', status: 'Planning', description: '' };
}

function progressTone(progress: number): string {
  if (progress >= 75) return 'is-success';
  if (progress >= 40) return '';
  return 'is-warning';
}

@Component({
  selector: 'app-projects',
  imports: [GridModule, Modal, IconComponent],
  providers: [PageService, SortService, ResizeService],
  templateUrl: './projects.html',
  styleUrl: './projects.css',
})
export class Projects implements OnInit {
  readonly statusOptions = statusOptions;
  readonly newProjectStatusOptions = newProjectStatusOptions;
  readonly statusBadgeClass = statusBadgeClass;
  readonly formatDate = formatDate;
  readonly formatCurrency = formatCurrency;
  readonly progressTone = progressTone;

  // Template-side lookups (Syncfusion's <ng-template let-p> context is `any`,
  // so `statusBadgeClass[p.status]` triggers TS7053 under strict template
  // checking). This wrapper accepts a `string` and casts on the TS side.
  statusBadge(s: string): string { return statusBadgeClass[s as ProjectStatus] ?? ''; }

  private route = inject(ActivatedRoute);
  private router = inject(Router);

  projects = signal<ProjectSummaryDto[]>([]);
  status = signal<ProjectStatus | 'All'>('All');
  search = signal('');
  loading = signal(true);
  error = signal<string | null>(null);

  showNewProjectModal = signal(false);
  newProjectDraft = signal<NewProjectDraft>(emptyProjectDraft());

  filteredProjects = computed(() => {
    const q = this.search().trim().toLowerCase();
    const status = this.status();
    return this.projects().filter((p) => {
      const matchesStatus = status === 'All' || p.status === status;
      const matchesSearch =
        !q || p.name.toLowerCase().includes(q) || p.code.toLowerCase().includes(q) || (p.location ?? '').toLowerCase().includes(q);
      return matchesStatus && matchesSearch;
    });
  });

  constructor(private projectsApi: ProjectsService) {
    // Read ?status=Active from URL into the local signal (deep-link from Dashboard).
    const param = this.route.snapshot.queryParamMap.get('status');
    if (param && (statusOptions as string[]).includes(param)) {
      this.status.set(param as ProjectStatus | 'All');
    }
    // Push signal → URL whenever the user picks a different filter from the
    // dropdown. Mirrors React's `setStatusFilter` URL-as-source-of-truth.
    effect(() => {
      const next = this.status();
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { status: next === 'All' ? null : next },
        queryParamsHandling: 'merge',
        replaceUrl: true,
      });
    });
  }

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects(): void {
    this.loading.set(true);
    this.error.set(null);
    this.projectsApi.getProjects({ page: 1, pageSize: 1000 }).subscribe({
      next: (result) => {
        this.projects.set(result.data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err instanceof Error ? err.message : 'Failed to load projects');
        this.loading.set(false);
      },
    });
  }

  onSearchChange(value: string): void {
    this.search.set(value);
  }

  onStatusChange(value: ProjectStatus | 'All'): void {
    this.status.set(value);
  }

  goToProject(id: number): void {
    this.router.navigate(['/projects', id]);
  }

  onRowSelected(args: RowSelectEventArgs): void {
    const data = args.data as ProjectSummaryDto | undefined;
    if (data) this.goToProject(data.id);
  }

  openNewProjectModal(): void {
    this.newProjectDraft.set(emptyProjectDraft());
    this.showNewProjectModal.set(true);
  }

  updateDraft(patch: Partial<NewProjectDraft>): void {
    this.newProjectDraft.update((d) => ({ ...d, ...patch }));
  }

  handleSaveNewProject(): void {
    const draft = this.newProjectDraft();
    if (!draft.name.trim()) return;
    const projects = this.projects();
    const nextId = projects.length ? Math.max(...projects.map((p) => p.id)) + 1 : 1;
    const created: ProjectSummaryDto = {
      id: nextId,
      name: draft.name.trim(),
      code: draft.code.trim() || `PRJ-${String(nextId).padStart(4, '0')}`,
      description: draft.description.trim() || undefined,
      startDate: draft.startDate || new Date().toISOString(),
      endDate: draft.endDate || draft.startDate || new Date().toISOString(),
      status: draft.status,
      location: draft.location.trim() || undefined,
      budget: Number(draft.budget) || 0,
      progress: 0,
      manager: draft.manager.trim() || undefined,
      createdDate: new Date().toISOString(),
      healthStatus: 'NotStarted',
    };
    // Demo only: kept in local component state so it's visible in the UI immediately;
    // nothing is written back to the API.
    this.projects.set([created, ...projects]);
    this.status.set('All');
    this.search.set('');
    this.showNewProjectModal.set(false);
  }

  handleExportProjects(): void {
    downloadCsv<ProjectSummaryDto>(
      'projects',
      [
        { header: 'Project ID', value: (p) => p.code },
        { header: 'Name', value: (p) => p.name },
        { header: 'Location', value: (p) => p.location ?? '' },
        { header: 'Start Date', value: (p) => formatDate(p.startDate) },
        { header: 'Finish Date', value: (p) => formatDate(p.endDate) },
        { header: 'Progress (%)', value: (p) => Math.round(p.progress) },
        { header: 'Budget', value: (p) => p.budget },
        { header: 'Status', value: (p) => p.status },
      ],
      this.filteredProjects(),
    );
  }
}
