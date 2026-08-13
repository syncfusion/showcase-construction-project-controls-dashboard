import type { ReactElement } from 'react';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { GridComponent, ColumnsDirective, ColumnDirective, Inject, Page, Resize } from '@syncfusion/ej2-react-grids';
import { projectsApi } from '../api/reports';
import { Modal } from '../components/Modal';
import { Icon } from '../components/Icon';
import type { ProjectSummaryDto, ProjectStatus } from '../types';
import { downloadCsv } from '../utils/csv';
import { format as formatDate } from '../utils/date';
import { formatCurrency } from '../utils/format';
import './ProjectsPage.css';

const statusOptions: (ProjectStatus | 'All')[] = ['All', 'Active', 'Planning', 'OnHold', 'Completed', 'Cancelled'];
const newProjectStatusOptions: ProjectStatus[] = ['Planning', 'Active', 'OnHold', 'Completed', 'Cancelled'];

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
  return {
    name: '',
    code: '',
    location: '',
    startDate: today,
    endDate: today,
    budget: '',
    manager: '',
    status: 'Planning',
    description: '',
  };
}

const statusBadgeClass: Record<ProjectStatus, string> = {
  Active: 'badge-success',
  Planning: 'badge-info',
  OnHold: 'badge-warning',
  Completed: 'badge-info',
  Cancelled: 'badge-neutral',
};

function ProgressBar({ value }: { value: number }): ReactElement {
  // ProjectSummaryDto.progress is stored as 0–100 percent, not a 0–1 ratio.
  const progress = Math.max(0, Math.min(100, Math.round(value)));
  const tone = progress >= 75 ? 'is-success' : progress >= 40 ? '' : 'is-warning';
  return (
    <div>
      <div className="progress-bar">
        <div className={`progress-fill ${tone}`} style={{ width: `${progress}%` }} />
      </div>
      <span className="text-secondary" style={{ fontSize: 'var(--text-caption-size)' }}>{progress}%</span>
    </div>
  );
}

function StatusBadge({ status }: { status: ProjectStatus }): ReactElement {
  return <span className={`badge ${statusBadgeClass[status]}`}>{status}</span>;
}

export function ProjectsPage(): ReactElement {
  const navigate = useNavigate();
  // Status filter is driven by the URL query string (?status=Active) so
  // other pages (e.g. the Dashboard "Active Projects" KPI card) can deep-link
  // into a pre-filtered view. Falls back to 'All' when the param is absent or
  // invalid. The select below writes via setSearchParams; the URL is the
  // single source of truth — making the filter shareable and bookmarkable.
  const [searchParams, setSearchParams] = useSearchParams();
  const statusParam = searchParams.get('status');
  const status: ProjectStatus | 'All' = (statusOptions as string[]).includes(statusParam ?? '')
    ? (statusParam as ProjectStatus | 'All')
    : 'All';

  function setStatusFilter(next: ProjectStatus | 'All'): void {
    setSearchParams((prev) => {
      if (next === 'All') prev.delete('status');
      else prev.set('status', next);
      return prev;
    }, { replace: true });
  }

  const [projects, setProjects] = useState<ProjectSummaryDto[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showNewProjectModal, setShowNewProjectModal] = useState(false);
  const [newProjectDraft, setNewProjectDraft] = useState<NewProjectDraft>(emptyProjectDraft);
  const projectsGridRef = useRef<GridComponent>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    projectsApi.getProjects({ page: 1, pageSize: 1000 })
      .then((result) => {
        if (cancelled) return;
        setProjects(result.data);
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Failed to load projects');
        setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const filteredProjects = useMemo(() => {
    const q = search.trim().toLowerCase();
    return projects.filter((p) => {
      const matchesStatus = status === 'All' || p.status === status;
      const matchesSearch =
        !q ||
        p.name.toLowerCase().includes(q) ||
        p.code.toLowerCase().includes(q) ||
        (p.location ?? '').toLowerCase().includes(q);
      return matchesStatus && matchesSearch;
    });
  }, [projects, status, search]);

  function openNewProjectModal(): void {
    setNewProjectDraft(emptyProjectDraft());
    setShowNewProjectModal(true);
  }

  function handleSaveNewProject(): void {
    if (!newProjectDraft.name.trim()) return;
    const nextId = projects.length ? Math.max(...projects.map((p) => p.id)) + 1 : 1;
    const draft: ProjectSummaryDto = {
      id: nextId,
      name: newProjectDraft.name.trim(),
      code: newProjectDraft.code.trim() || `PRJ-${String(nextId).padStart(4, '0')}`,
      description: newProjectDraft.description.trim() || undefined,
      startDate: newProjectDraft.startDate || new Date().toISOString(),
      endDate: newProjectDraft.endDate || newProjectDraft.startDate || new Date().toISOString(),
      status: newProjectDraft.status,
      location: newProjectDraft.location.trim() || undefined,
      budget: Number(newProjectDraft.budget) || 0,
      progress: 0,
      manager: newProjectDraft.manager.trim() || undefined,
      createdDate: new Date().toISOString(),
      healthStatus: 'NotStarted',
    };
    // Demo only: kept in local component state so it's visible in the UI immediately;
    // nothing is written back to the API.
    setProjects((prev) => [draft, ...prev]);
    setStatusFilter('All');
    setSearch('');
    setShowNewProjectModal(false);
  }

  function handleExportProjects(): void {
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
      filteredProjects,
    );
  }

  return (
    <div className="projects-page">
      <div className="toolbar">
        <div className="toolbar-left">
          <div className="input-with-icon">
            <Icon className="icon" name="search" size={16} />
            <input
              type="search"
              className="input"
              placeholder="Search projects…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{ minWidth: 240 }}
            />
          </div>
          <select
            className="select"
            value={status}
            onChange={(e) => setStatusFilter(e.target.value as ProjectStatus | 'All')}
            aria-label="Filter projects by status"
            style={{ minWidth: 160 }}
          >
            {statusOptions.map((s) => (
              <option key={s} value={s}>{s === 'All' ? 'All statuses' : s}</option>
            ))}
          </select>
        </div>
        <div className="toolbar-right">
          <button type="button" className="btn btn-secondary btn-sm" onClick={handleExportProjects} disabled={filteredProjects.length === 0}>
            <Icon className="icon" name="download" size={14} />
            Export
          </button>
          <button type="button" className="btn btn-primary" onClick={openNewProjectModal}>
            <Icon className="icon" name="plus" size={14} />
            New Project
          </button>
        </div>
      </div>

      {loading && <div className="loading-state">Loading projects…</div>}
      {error && <div className="alert alert-error" role="alert">{error}</div>}

      {!loading && !error && (
        <div className="grid-card">
        <GridComponent
          ref={projectsGridRef}
          dataSource={filteredProjects}
          allowPaging={true}
          allowResizing={true}
          width="100%"
          pageSettings={{ pageSize: 20, pageCount: 4 }}
          gridLines="Horizontal"
          rowSelected={(args) => { if (args.data) navigate(`/projects/${(args.data as ProjectSummaryDto).id}`); }}
        >
          <ColumnsDirective>
            <ColumnDirective
              field="code"
              headerText="Project ID"
              width="105"
              template={(p: ProjectSummaryDto) => <span className="font-mono cell-clamp-2">{p.code}</span>}
            />
            <ColumnDirective
              field="name"
              headerText="Name"
              minWidth="120"
              template={(p: ProjectSummaryDto) => (
                <span style={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {p.name}
                </span>
              )}
            />
            <ColumnDirective
              field="location"
              headerText="Location"
              width="130"
              template={(p: ProjectSummaryDto) => <span className="cell-clamp-2">{p.location ?? '—'}</span>}
            />
            <ColumnDirective
              field="startDate"
              headerText="Start Date"
              width="95"
              template={(p: ProjectSummaryDto) => <span className="cell-clamp-2">{formatDate(p.startDate)}</span>}
            />
            <ColumnDirective
              field="endDate"
              headerText="Finish Date"
              width="95"
              template={(p: ProjectSummaryDto) => <span className="cell-clamp-2">{formatDate(p.endDate)}</span>}
            />
            <ColumnDirective
              field="progress"
              headerText="Progress"
              width="130"
              template={(p: ProjectSummaryDto) => <ProgressBar value={p.progress} />}
            />
            <ColumnDirective
              field="budget"
              headerText="Budget"
              width="110"
              template={(p: ProjectSummaryDto) => <span className="cell-clamp-2">{formatCurrency(p.budget)}</span>}
            />
            <ColumnDirective
              field="status"
              headerText="Status"
              width="110"
              template={(p: ProjectSummaryDto) => <StatusBadge status={p.status} />}
            />
            <ColumnDirective
              headerText=""
              width="48"
              template={() => (
                <button type="button" className="btn btn-ghost btn-icon btn-sm" tabIndex={-1} aria-hidden="true">
                  <Icon className="icon" name="chevron-right" size={14} />
                </button>
              )}
            />
          </ColumnsDirective>
          <Inject services={[Page, Resize]} />
        </GridComponent>
        </div>
      )}

      <Modal
        open={showNewProjectModal}
        onClose={() => setShowNewProjectModal(false)}
        title="New Project"
        subtitle="Demo template — saved to this view only; nothing is sent to the backend."
        size="lg"
        footer={
          <>
            <button type="button" className="btn btn-secondary" onClick={() => setShowNewProjectModal(false)}>
              Cancel
            </button>
            <button
              type="button"
              className="btn btn-primary"
              onClick={handleSaveNewProject}
              disabled={!newProjectDraft.name.trim()}
            >
              Save Project
            </button>
          </>
        }
      >
        <div className="modal-form-grid">
          <div className="modal-form-field is-span-2">
            <label htmlFor="proj-name">Project Name</label>
            <input
              id="proj-name"
              className="input"
              type="text"
              value={newProjectDraft.name}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, name: e.target.value }))}
              required
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-code">Project Code</label>
            <input
              id="proj-code"
              className="input"
              type="text"
              value={newProjectDraft.code}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, code: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-status">Status</label>
            <select
              id="proj-status"
              className="select"
              value={newProjectDraft.status}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, status: e.target.value as ProjectStatus }))}
            >
              {newProjectStatusOptions.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-location">Location</label>
            <input
              id="proj-location"
              className="input"
              type="text"
              value={newProjectDraft.location}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, location: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-manager">Project Manager</label>
            <input
              id="proj-manager"
              className="input"
              type="text"
              value={newProjectDraft.manager}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, manager: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-start">Start Date</label>
            <input
              id="proj-start"
              className="input"
              type="date"
              value={newProjectDraft.startDate}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, startDate: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-end">Finish Date</label>
            <input
              id="proj-end"
              className="input"
              type="date"
              value={newProjectDraft.endDate}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, endDate: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="proj-budget">Budget ($)</label>
            <input
              id="proj-budget"
              className="input"
              type="number"
              value={newProjectDraft.budget}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, budget: e.target.value }))}
            />
          </div>
          <div className="modal-form-field is-span-2">
            <label htmlFor="proj-description">Description</label>
            <textarea
              id="proj-description"
              className="input"
              rows={3}
              value={newProjectDraft.description}
              onChange={(e) => setNewProjectDraft((d) => ({ ...d, description: e.target.value }))}
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}
