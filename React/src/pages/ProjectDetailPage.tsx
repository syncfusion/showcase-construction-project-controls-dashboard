import type { ReactElement } from 'react';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useOutletContext, useParams } from 'react-router-dom';
import { GridComponent, ColumnsDirective, ColumnDirective, Inject, Resize } from '@syncfusion/ej2-react-grids';
import {
  PdfViewerComponent,
  Toolbar,
  Magnification,
  Navigation,
  LinkAnnotation,
  BookmarkView,
  ThumbnailView,
  Print,
  TextSelection,
  TextSearch,
  Inject as PdfViewerInject,
} from '@syncfusion/ej2-react-pdfviewer';
import { format } from '../utils/date';
import { formatCurrency } from '../utils/format';
import { projectsApi } from '../api/reports';
import { getPublicAssetUrl } from '../basePath';
import { Modal } from '../components/Modal';
import { Icon } from '../components/Icon';
import type {
  ProjectDetailDto,
  ProjectKpisDto,
  RecentDocumentDto,
  RiskSummaryDto,
  ProjectMilestoneDto,
  ChangeOrderSummaryDto,
  RfiSummaryDto,
  SubmittalSummaryDto,
  HealthStatus,
  TaskStatus,
} from '../types';
import type { ShellOutletContext } from '../layout/Shell';
import './ProjectDetailPage.css';

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

interface LoadingErrorStateProps {
  loading: boolean;
  error: string | null;
}
function LoadingErrorState({ loading, error }: LoadingErrorStateProps): ReactElement | null {
  if (error) return <div className="alert alert-error" role="alert">{error}</div>;
  if (loading) return <div className="loading-state" aria-live="polite">Loading project details…</div>;
  return null;
}

type DetailTab = 'Overview' | 'Schedule' | 'Cost' | 'RFIs' | 'Submittals';

function tabBadge(tab: DetailTab, kpis: ProjectKpisDto): number {
  switch (tab) {
    case 'RFIs':
      return kpis.openRfis;
    case 'Submittals':
      return kpis.openSubmittals;
    default:
      return 0;
  }
}

function formatPercent(n: number): string {
  return `${n > 0 ? '+' : ''}${n}%`;
}

export function ProjectDetailPage(): ReactElement {
  const navigate = useNavigate();
  const { setTopbarHeading } = useOutletContext<ShellOutletContext>();
  const { id } = useParams<{ id: string }>();
  const [project, setProject] = useState<ProjectDetailDto | null>(null);
  const [kpis, setKpis] = useState<ProjectKpisDto | null>(null);
  const [milestones, setMilestones] = useState<ProjectMilestoneDto[]>([]);
  const [risks, setRisks] = useState<RiskSummaryDto[]>([]);
  const [documents, setDocuments] = useState<RecentDocumentDto[]>([]);
  const [rfis, setRfis] = useState<RfiSummaryDto[]>([]);
  const [submittals, setSubmittals] = useState<SubmittalSummaryDto[]>([]);
  const [changeOrders, setChangeOrders] = useState<ChangeOrderSummaryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<DetailTab>('Overview');

  const projectId = useMemo(() => Number(id), [id]);

  useEffect(() => {
    if (Number.isNaN(projectId)) {
      setError('Invalid project ID');
      setLoading(false);
      return;
    }
    let cancelled = false;
    setLoading(true);
    setError(null);

    Promise.all([
      projectsApi.getById(projectId),
      projectsApi.getKpis(projectId),
      projectsApi.getUpcomingMilestones(projectId, 30, 10),
      projectsApi.getTopRisks(projectId, 5),
      projectsApi.getRecentDocuments(projectId, 30, 10),
      projectsApi.getRfis(projectId, 50),
      projectsApi.getSubmittals(projectId, 50),
      projectsApi.getChangeOrders(projectId, 50),
    ])
      .then(([projectData, kpisData, milestonesData, risksData, documentsData, rfisData, submittalsData, changeOrdersData]) => {
        if (cancelled) return;
        setProject(projectData);
        setKpis(kpisData);
        setMilestones(milestonesData);
        setRisks(risksData);
        setDocuments(documentsData);
        setRfis(rfisData);
        setSubmittals(submittalsData);
        setChangeOrders(changeOrdersData);
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Failed to load project details');
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [projectId]);

  const headerSubtitle = useMemo(() => {
    if (!project) return '';
    const parts = [
      project.location,
      project.startDate && project.endDate ? `${format(project.startDate)} – ${format(project.endDate)}` : null,
      project.budget != null ? `${formatCurrency(project.budget)} budget` : null,
    ].filter(Boolean);
    return parts.join(' · ');
  }, [project]);

  const tabs: DetailTab[] = useMemo(() => ['Overview', 'Schedule', 'Cost', 'RFIs', 'Submittals'], []);

  // Push only the Back button + breadcrumb into the topbar (Projects / code).
  // The project name and subtitle stay in the page content as a slim header.
  // Cleared by the Shell on route change.
  useEffect(() => {
    if (!project) {
      setTopbarHeading(null);
      return;
    }
    setTopbarHeading(
      <div className="topbar-breadcrumb">
        <button
          type="button"
          className="btn btn-ghost btn-icon btn-sm"
          onClick={() => navigate('/projects')}
          aria-label="Back to projects"
        >
          <Icon className="icon" name="arrow-left" size={16} />
        </button>
        <span>Projects</span>
        <span aria-hidden="true">/</span>
        <span className="font-mono">{project.code}</span>
      </div>
    );
  }, [project, navigate, setTopbarHeading]);

  return (
    <div className="project-detail-page">
      <LoadingErrorState loading={loading} error={error} />

      {!loading && !error && project && kpis && (
        <>
          <header className="project-title">
            <h1>{project.name}</h1>
            {headerSubtitle && <p>{headerSubtitle}</p>}
          </header>

          <div className="tabs" role="tablist" aria-label="Project detail tabs">
            {tabs.map((tab) => {
              const badge = tabBadge(tab, kpis);
              return (
                <button
                  key={tab}
                  type="button"
                  role="tab"
                  aria-selected={activeTab === tab}
                  className={`tab${activeTab === tab ? ' is-active' : ''}`}
                  onClick={() => setActiveTab(tab)}
                >
                  {tab}
                  {badge > 0 && <span className="badge badge-error" style={{ marginLeft: 8 }}>{badge}</span>}
                </button>
              );
            })}
          </div>

          {activeTab === 'Overview' && (
            <OverviewTab
              project={project}
              kpis={kpis}
              milestones={milestones}
              risks={risks}
              documents={documents}
            />
          )}
          {activeTab === 'Schedule' && <ScheduleTab milestones={milestones} />}
          {activeTab === 'Cost' && <CostTab kpis={kpis} changeOrders={changeOrders} />}
          {activeTab === 'RFIs' && <RfisTab rfis={rfis} />}
          {activeTab === 'Submittals' && <SubmittalsTab submittals={submittals} />}
        </>
      )}
    </div>
  );
}

interface OverviewTabProps {
  project: ProjectDetailDto;
  kpis: ProjectKpisDto;
  milestones: ProjectMilestoneDto[];
  risks: RiskSummaryDto[];
  documents: RecentDocumentDto[];
}

function OverviewTab({ project, kpis, milestones, risks, documents }: OverviewTabProps): ReactElement {
  //const navigate = useNavigate();
  const [previewDocument, setPreviewDocument] = useState<RecentDocumentDto | null>(null);
  const overviewMilestonesGridRef = useRef<GridComponent>(null);
  const overviewDocumentsGridRef = useRef<GridComponent>(null);
  return (
    <>
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-label">Percent Complete</div>
          <div className="kpi-value">{kpis.percentComplete}%</div>
          <div className="progress-bar">
            <div
              className={`progress-fill ${kpis.percentComplete >= 75 ? 'is-success' : kpis.percentComplete >= 40 ? 'is-warning' : 'is-error'}`}
              style={{ width: `${Math.min(100, Math.max(0, kpis.percentComplete))}%` }}
            />
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Cost Variance</div>
          <div className={`kpi-value ${kpis.costVariance >= 0 ? 'text-success' : 'text-error'}`}>
            {kpis.costVariance >= 0 ? '+' : '-'}
            {formatCurrency(Math.abs(kpis.costVariance))}
          </div>
          <div className={`kpi-change ${kpis.costVariance >= 0 ? 'positive' : 'negative'}`}>
            <Icon className="icon" name={kpis.costVariance >= 0 ? 'arrow-up-right' : 'arrow-down-right'} size={14} />
            {kpis.costVariance >= 0 ? 'Under budget' : 'Over budget'}
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Schedule Variance</div>
          <div className={`kpi-value ${kpis.scheduleVariance >= 0 ? 'text-success' : 'text-error'}`}>
            {formatPercent(kpis.scheduleVariance)}
          </div>
          <div className={`kpi-change ${kpis.scheduleVariance >= 0 ? 'positive' : 'negative'}`}>
            <Icon className="icon" name={kpis.scheduleVariance >= 0 ? 'arrow-up-right' : 'arrow-down-right'} size={14} />
            {kpis.scheduleVariance >= 0 ? 'Ahead of schedule' : 'Behind schedule'}
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Open RFIs</div>
          <div className="kpi-value">{kpis.openRfis}</div>
          <div className={`kpi-change ${kpis.overdueRfis > 0 ? 'warning' : 'positive'}`}>
            <Icon className="icon" name={kpis.overdueRfis > 0 ? 'clock' : 'check-circle'} size={14} />
            {kpis.overdueRfis > 0 ? `${kpis.overdueRfis} overdue` : 'No overdue'}
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Open Submittals</div>
          <div className="kpi-value">{kpis.openSubmittals}</div>
          <div className={`kpi-change ${kpis.overdueSubmittals > 0 ? 'warning' : 'positive'}`}>
            <Icon className="icon" name={kpis.overdueSubmittals > 0 ? 'clock' : 'check-circle'} size={14} />
            {kpis.overdueSubmittals > 0 ? `${kpis.overdueSubmittals} overdue` : 'On track'}
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Pending Change Orders</div>
          <div className="kpi-value">{formatCurrency(kpis.pendingChangeOrdersAmount)}</div>
          <div className={`kpi-change ${kpis.pendingChangeOrdersAmount > 0 ? 'warning' : 'positive'}`}>
            <Icon className="icon" name={kpis.pendingChangeOrdersAmount > 0 ? 'file-plus' : 'check-circle'} size={14} />
            {kpis.pendingChangeOrdersAmount > 0 ? `${kpis.openChangeOrders} pending review` : 'No pending COs'}
          </div>
        </div>
      </div>

      <div className="panel-grid">
        <div className="card grid-card">
          <div className="card-header">
            <div>
              <h2 className="card-title">Upcoming Milestones</h2>
              <p className="card-subtitle">Next 30 days</p>
            </div>
          </div>
          <GridComponent
            ref={overviewMilestonesGridRef}
            dataSource={milestones}
            allowResizing={true}
            width="100%"
            gridLines="Horizontal"
          >
            <ColumnsDirective>
              <ColumnDirective
                field="title"
                headerText="Milestone"
                template={(m: ProjectMilestoneDto) => (
                  <span style={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {m.title}
                  </span>
                )}
              />
              <ColumnDirective
                field="plannedDate"
                headerText="Date"
                width="100"
                template={(m: ProjectMilestoneDto) => <span className="cell-clamp-2">{format(m.plannedDate)}</span>}
              />
              <ColumnDirective
                field="status"
                headerText="Status"
                width="110"
                template={(m: ProjectMilestoneDto) => (
                  <span className={`badge ${milestoneStatusBadgeClass[m.status]}`}>{milestoneStatusLabel[m.status]}</span>
                )}
              />
            </ColumnsDirective>
            <Inject services={[Resize]} />
          </GridComponent>
        </div>

        <div className="card">
          <div className="card-header">
            <div>
              <h2 className="card-title">Risk Register</h2>
              <p className="card-subtitle">Top open items</p>
            </div>
          </div>
          <div className="card-body">
            {risks.length === 0 && (
              <div className="text-secondary" style={{ textAlign: 'center' }}>No open risks</div>
            )}
            {risks.map((r) => (
              <div key={r.id} className={`alert ${riskAlertClass[r.severity] ?? 'alert-info'}`}>
                <Icon className="icon" name={r.severity === 'Critical' ? 'shield-alert' : 'alert-triangle'} size={16} />
                <div>
                  <div style={{ fontWeight: 600 }}>{r.title}</div>
                  <div style={{ fontSize: 'var(--text-caption-size)', opacity: 0.9 }}>
                    {r.impactDisplay}
                    {r.owner ? ` · Owner: ${r.owner}` : ''}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="card grid-card">
        <div className="card-header">
          <div>
            <h2 className="card-title">Document Workflow</h2>
            <p className="card-subtitle">Last 30 days · {project.code}</p>
          </div>
        </div>
        <GridComponent
          ref={overviewDocumentsGridRef}
          dataSource={documents}
          allowResizing={true}
          width="100%"
          gridLines="Horizontal"
          rowSelected={(args) => { if (args.data) setPreviewDocument(args.data as RecentDocumentDto); }}
        >
          <ColumnsDirective>
            <ColumnDirective
              field="title"
              headerText="Document"
              template={(d: RecentDocumentDto) => (
                <span style={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {d.title}
                </span>
              )}
            />
            <ColumnDirective
              field="type"
              headerText="Type"
              width="130"
              template={(d: RecentDocumentDto) => <span className="cell-clamp-2">{d.type}</span>}
            />
            <ColumnDirective
              field="revision"
              headerText="Revision"
              width="90"
              template={(d: RecentDocumentDto) => <span className="font-mono cell-clamp-2">{d.revision ?? '—'}</span>}
            />
            <ColumnDirective
              field="submittedDate"
              headerText="Submitted"
              width="120"
              template={(d: RecentDocumentDto) => <span className="cell-clamp-2">{format(d.submittedDate)}</span>}
            />
            <ColumnDirective
              field="status"
              headerText="Status"
              width="130"
              template={(d: RecentDocumentDto) => (
                <span className={`badge ${documentStatusClass(d.status)}`}>{d.status}</span>
              )}
            />
          </ColumnsDirective>
          <Inject services={[Resize]} />
        </GridComponent>
      </div>

      <div className="card" style={{ marginTop: 'var(--space-xl)' }}>
        <div className="card-header">
          <div>
            <h2 className="card-title">Project Details</h2>
            <p className="card-subtitle">General information</p>
          </div>
        </div>
        <ul className="meta-list">
          <li>
            <span className="meta-label">Status</span>
            <span className={`badge ${healthBadgeClass[project.healthStatus]}`}>{project.status}</span>
          </li>
          <li>
            <span className="meta-label">Health</span>
            <span className={`badge ${healthBadgeClass[project.healthStatus]}`}>
              {project.healthStatus.replace(/([A-Z])/g, ' $1').trim()}
            </span>
          </li>
          <li>
            <span className="meta-label">Project Manager</span>
            <span className="meta-value">{project.manager ?? '—'}</span>
          </li>
          <li>
            <span className="meta-label">Duration</span>
            <span className="meta-value">{format(project.startDate)} – {format(project.endDate)}</span>
          </li>
          <li>
            <span className="meta-label">Location</span>
            <span className="meta-value">{project.location ?? '—'}</span>
          </li>
          <li>
            <span className="meta-label">Budget</span>
            <span className="meta-value">{formatCurrency(project.budget)}</span>
          </li>
        </ul>
      </div>

      <Modal
        open={!!previewDocument}
        onClose={() => setPreviewDocument(null)}
        title={previewDocument?.title ?? ''}
        subtitle={previewDocument ? `${previewDocument.type} · Rev ${previewDocument.revision ?? '—'} · Submitted ${format(previewDocument.submittedDate)}` : undefined}
        size="xl"
      >
        {previewDocument && (
          <>
            <div className="modal-detail-grid">
              <div className="modal-detail-item">
                <span className="modal-detail-label">Document #</span>
                <span className="modal-detail-value font-mono">{previewDocument.documentNumber}</span>
              </div>
              <div className="modal-detail-item">
                <span className="modal-detail-label">Status</span>
                <span className={`badge ${documentStatusClass(previewDocument.status)}`} style={{ width: 'fit-content' }}>
                  {previewDocument.status}
                </span>
              </div>
            </div>
            <div style={{ height: 620, borderRadius: 'var(--radius-md)', overflow: 'hidden' }}>
              <PdfViewerComponent
                id="project-document-pdf-viewer"
                documentPath={SAMPLE_PDF_URL}
                resourceUrl={getPublicAssetUrl('ej2-pdfviewer-lib')}
                style={{ height: '620px' }}
              >
                <PdfViewerInject
                  services={[
                    Toolbar,
                    Magnification,
                    Navigation,
                    LinkAnnotation,
                    BookmarkView,
                    ThumbnailView,
                    Print,
                    TextSelection,
                    TextSearch,
                  ]}
                />
              </PdfViewerComponent>
            </div>
          </>
        )}
      </Modal>
    </>
  );
}

interface ScheduleTabProps {
  milestones: ProjectMilestoneDto[];
}

function ScheduleTab({ milestones }: ScheduleTabProps): ReactElement {
  const gridRef = useRef<GridComponent>(null);
  return (
    <div className="grid-card" style={{ marginTop: 'var(--space-lg)' }}>
      <GridComponent
        ref={gridRef}
        dataSource={milestones}
        allowResizing={true}
        width="100%"
        gridLines="Horizontal"
      >
        <ColumnsDirective>
          <ColumnDirective
            field="title"
            headerText="Milestone"
            template={(m: ProjectMilestoneDto) => (
              <span style={{ fontWeight: 600, display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                {m.title}
              </span>
            )}
          />
          <ColumnDirective
            field="description"
            headerText="Description"
            template={(m: ProjectMilestoneDto) => (
              <span className="text-secondary cell-clamp-2">
                {m.description ?? '—'}
              </span>
            )}
          />
          <ColumnDirective
            field="plannedDate"
            headerText="Planned Date"
            width="130"
            template={(m: ProjectMilestoneDto) => <span className="cell-clamp-2">{format(m.plannedDate)}</span>}
          />
          <ColumnDirective
            field="status"
            headerText="Status"
            width="130"
            template={(m: ProjectMilestoneDto) => (
              <span className={`badge ${milestoneStatusBadgeClass[m.status]}`}>{milestoneStatusLabel[m.status]}</span>
            )}
          />
          <ColumnDirective
            field="owner"
            headerText="Owner"
            width="160"
            template={(m: ProjectMilestoneDto) => <span className="cell-clamp-2">{m.owner ?? '—'}</span>}
          />
        </ColumnsDirective>
        <Inject services={[Resize]} />
      </GridComponent>
    </div>
  );
}

function CostTab({ kpis, changeOrders }: { kpis: ProjectKpisDto; changeOrders: ChangeOrderSummaryDto[] }): ReactElement {
  const costGridRef = useRef<GridComponent>(null);
  return (
    <>
      <div className="kpi-grid" style={{ marginTop: 'var(--space-lg)' }}>
        <div className="kpi-card">
          <div className="kpi-label">Budget</div>
          <div className="kpi-value">{formatCurrency(kpis.budget)}</div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Spent</div>
          <div className="kpi-value">{formatCurrency(kpis.spent)}</div>
          <div className="progress-bar">
            <div
              className={`progress-fill ${kpis.spent / kpis.budget > 0.9 ? 'is-error' : 'is-success'}`}
              style={{ width: `${Math.min(100, Math.round((kpis.spent / kpis.budget) * 100))}%` }}
            />
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Cost Variance</div>
          <div className={`kpi-value ${kpis.costVariance >= 0 ? 'text-success' : 'text-error'}`}>
            {kpis.costVariance >= 0 ? '+' : '-'}
            {formatCurrency(Math.abs(kpis.costVariance))}
          </div>
        </div>
        <div className="kpi-card">
          <div className="kpi-label">Pending Change Orders</div>
          <div className="kpi-value">{formatCurrency(kpis.pendingChangeOrdersAmount)}</div>
          <div className={`kpi-change ${kpis.pendingChangeOrdersAmount > 0 ? 'warning' : 'positive'}`}>
            {kpis.openChangeOrders} open
          </div>
        </div>
      </div>

      <div className="card grid-card">
        <div className="card-header">
          <div>
            <h2 className="card-title">Change Orders</h2>
            <p className="card-subtitle">Pending and approved project changes</p>
          </div>
        </div>
        <GridComponent
          ref={costGridRef}
          dataSource={changeOrders}
          allowResizing={true}
          width="100%"
          gridLines="Horizontal"
        >
          <ColumnsDirective>
            <ColumnDirective
              field="number"
              headerText="CO #"
              width="90"
              template={(co: ChangeOrderSummaryDto) => <span className="font-mono cell-clamp-2">{co.number}</span>}
            />
            <ColumnDirective
              field="description"
              headerText="Description"
              template={(co: ChangeOrderSummaryDto) => (
                <span style={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {co.description}
                </span>
              )}
            />
            <ColumnDirective
              field="amount"
              headerText="Amount"
              width="130"
              template={(co: ChangeOrderSummaryDto) => <span className="cell-clamp-2" style={{ fontWeight: 600 }}>{formatCurrency(co.amount)}</span>}
            />
            <ColumnDirective
              field="status"
              headerText="Status"
              width="130"
              template={(co: ChangeOrderSummaryDto) => (
                <span className={`badge ${changeOrderStatusClass(co.status)}`}>{co.status}</span>
              )}
            />
            <ColumnDirective
              field="requestedBy"
              headerText="Requested By"
              width="160"
              template={(co: ChangeOrderSummaryDto) => <span className="cell-clamp-2">{co.requestedBy ?? '—'}</span>}
            />
          </ColumnsDirective>
          <Inject services={[Resize]} />
        </GridComponent>
      </div>
    </>
  );
}

function RfisTab({ rfis }: { rfis: RfiSummaryDto[] }): ReactElement {
  const gridRef = useRef<GridComponent>(null);
  return (
    <div className="grid-card" style={{ marginTop: 'var(--space-lg)' }}>
      <GridComponent
        ref={gridRef}
        dataSource={rfis}
        allowResizing={true}
        width="100%"
        gridLines="Horizontal"
      >
        <ColumnsDirective>
          <ColumnDirective
            field="number"
            headerText="RFI #"
            width="90"
            template={(r: RfiSummaryDto) => <span className="font-mono cell-clamp-2">{r.number}</span>}
          />
          <ColumnDirective
            field="subject"
            headerText="Subject"
            template={(r: RfiSummaryDto) => (
              <span style={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                {r.subject}
              </span>
            )}
          />
          <ColumnDirective
            field="discipline"
            headerText="Discipline"
            width="120"
            template={(r: RfiSummaryDto) => <span className="cell-clamp-2">{r.discipline ?? '—'}</span>}
          />
          <ColumnDirective
            field="impact"
            headerText="Impact"
            width="100"
            template={(r: RfiSummaryDto) => <span className="cell-clamp-2">{r.impact ?? '—'}</span>}
          />
          <ColumnDirective
            field="status"
            headerText="Status"
            width="110"
            template={(r: RfiSummaryDto) => (
              <span className={`badge ${rfiStatusClass(r.status)}`}>{r.status}</span>
            )}
          />
          <ColumnDirective
            field="dueDate"
            headerText="Due"
            width="110"
            template={(r: RfiSummaryDto) => <span className="cell-clamp-2">{r.dueDate ? format(r.dueDate) : '—'}</span>}
          />
        </ColumnsDirective>
        <Inject services={[Resize]} />
      </GridComponent>
    </div>
  );
}

function SubmittalsTab({ submittals }: { submittals: SubmittalSummaryDto[] }): ReactElement {
  const gridRef = useRef<GridComponent>(null);
  return (
    <div className="grid-card" style={{ marginTop: 'var(--space-lg)' }}>
      <GridComponent
        ref={gridRef}
        dataSource={submittals}
        allowResizing={true}
        width="100%"
        gridLines="Horizontal"
      >
        <ColumnsDirective>
          <ColumnDirective
            field="number"
            headerText="Sub #"
            width="90"
            template={(s: SubmittalSummaryDto) => <span className="font-mono cell-clamp-2">{s.number}</span>}
          />
          <ColumnDirective
            field="title"
            headerText="Title"
            template={(s: SubmittalSummaryDto) => (
              <span style={{ display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                {s.title}
              </span>
            )}
          />
          <ColumnDirective
            field="submittalType"
            headerText="Type"
            width="120"
            template={(s: SubmittalSummaryDto) => <span className="cell-clamp-2">{s.submittalType ?? '—'}</span>}
          />
          <ColumnDirective
            field="discipline"
            headerText="Discipline"
            width="120"
            template={(s: SubmittalSummaryDto) => <span className="cell-clamp-2">{s.discipline ?? '—'}</span>}
          />
          <ColumnDirective
            field="specificationSection"
            headerText="Spec Section"
            width="120"
            template={(s: SubmittalSummaryDto) => <span className="cell-clamp-2">{s.specificationSection ?? '—'}</span>}
          />
          <ColumnDirective
            field="status"
            headerText="Status"
            width="130"
            template={(s: SubmittalSummaryDto) => (
              <span className={`badge ${submittalStatusClass(s.status)}`}>{s.status}</span>
            )}
          />
        </ColumnsDirective>
        <Inject services={[Resize]} />
      </GridComponent>
    </div>
  );
}

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
