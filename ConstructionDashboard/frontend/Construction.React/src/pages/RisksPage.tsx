import type { ReactElement } from 'react';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { GridComponent, ColumnsDirective, ColumnDirective, Inject, Page, Sort, Resize } from '@syncfusion/ej2-react-grids';
import { risksApi, riskMatrixApi } from '../api/reports';
import { Modal } from '../components/Modal';
import { Icon } from '../components/Icon';
import { RiskMatrixHeatmap } from '../components/RiskMatrixHeatmap';
import type { RiskKpisDto, RiskMatrixCellViewModel, RiskProbability, RiskSeverity, RiskStatus, RiskSummaryDto } from '../types';
import { onActivateKey } from '../utils/a11y';
import { downloadCsv } from '../utils/csv';
import { format as formatDate } from '../utils/date';
import './RisksPage.css';

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
    title: '',
    description: '',
    projectId: '',
    projectCode: '',
    severity: 'Medium',
    probability: 'Medium',
    owner: '',
    mitigationPlan: '',
    targetResolutionDate: '',
    impactDays: '',
    impactCost: '',
  };
}

const severityBadgeClass: Record<RiskSeverity, string> = {
  Critical: 'badge-error',
  High: 'badge-warning',
  Medium: 'badge-info',
  Low: 'badge-neutral',
};

const statusBadgeClass: Record<RiskStatus, string> = {
  Open: 'badge-error',
  InProgress: 'badge-warning',
  Monitoring: 'badge-info',
  Escalated: 'badge-error',
  Containment: 'badge-warning',
  Mitigated: 'badge-success',
  Closed: 'badge-neutral',
};

const kpiBorderColor: Record<'critical' | 'high' | 'medium' | 'mitigated', string> = {
  critical: 'var(--color-error)',
  high: 'var(--color-warning)',
  medium: 'var(--color-warning)',
  mitigated: 'var(--color-success)',
};

const kpiIcon: Record<'critical' | 'high' | 'medium' | 'mitigated', string> = {
  critical: 'shield-alert',
  high: 'alert-triangle',
  medium: 'activity',
  mitigated: 'check-circle',
};

const kpiChangeTone: Record<'critical' | 'high' | 'medium' | 'mitigated', string> = {
  critical: 'negative',
  high: 'warning',
  medium: 'text-secondary',
  mitigated: 'positive',
};

const kpiChangeLabel: Record<'critical' | 'high' | 'medium' | 'mitigated', string> = {
  critical: 'Immediate action required',
  high: 'Watch closely',
  medium: 'Monitored',
  mitigated: 'On track',
};

const severityOptions: (RiskSeverity | 'All')[] = ['All', 'Critical', 'High', 'Medium', 'Low'];
const statusOptions: (RiskStatus | 'All')[] = [
  'All',
  'Open',
  'InProgress',
  'Monitoring',
  'Escalated',
  'Containment',
  'Mitigated',
  'Closed',
];

function formatImpact(risk: RiskSummaryDto): string {
  const parts: string[] = [];
  if (risk.impactDays) parts.push(`${risk.impactDays}d`);
  if (risk.impactCost) parts.push(`$${(risk.impactCost / 1_000_000).toFixed(1)}M`);
  if (!parts.length) return 'Minor';
  return parts.join(' · ');
}

function useDebouncedValue<T>(value: T, delay = 250): T {
  const [debounced, setDebounced] = useState(value);
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);
  return debounced;
}

const severities: RiskSeverity[] = ['Low', 'Medium', 'High', 'Critical'];
const probabilities: RiskProbability[] = ['Low', 'Medium', 'High'];

export function RisksPage(): ReactElement {
  const navigate = useNavigate();
  // Severity and status filters are URL-driven (?severity=Critical&status=Open)
  // so the Dashboard's "Open Risks" KPI card can deep-link into a pre-filtered
  // view (the card surfaces "N critical", so the natural deep-link is
  // ?severity=Critical). The in-page Risk KPI cards (Critical/High/Medium/
  // Mitigated) also write through these setters so the URL stays in sync.
  // Falls back to 'All' when a param is absent or invalid.
  const [searchParams, setSearchParams] = useSearchParams();
  const severityParam = searchParams.get('severity');
  const statusParam = searchParams.get('status');
  const severity: RiskSeverity | 'All' = (severityOptions as string[]).includes(severityParam ?? '')
    ? (severityParam as RiskSeverity | 'All')
    : 'All';
  const status: RiskStatus | 'All' = (statusOptions as string[]).includes(statusParam ?? '')
    ? (statusParam as RiskStatus | 'All')
    : 'All';

  function setSeverityFilter(next: RiskSeverity | 'All'): void {
    setSearchParams((prev) => {
      if (next === 'All') prev.delete('severity');
      else prev.set('severity', next);
      return prev;
    }, { replace: true });
  }

  function setStatusFilter(next: RiskStatus | 'All'): void {
    setSearchParams((prev) => {
      if (next === 'All') prev.delete('status');
      else prev.set('status', next);
      return prev;
    }, { replace: true });
  }

  const [risks, setRisks] = useState<RiskSummaryDto[]>([]);
  const [kpis, setKpis] = useState<RiskKpisDto | null>(null);
  const [matrix, setMatrix] = useState<RiskMatrixCellViewModel[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const risksGridRef = useRef<GridComponent>(null);

  const [search, setSearch] = useState('');
  const debouncedSearch = useDebouncedValue(search);

  const [selectedRisk, setSelectedRisk] = useState<RiskSummaryDto | null>(null);
  // Cell click on the heatmap opens a cell-drill popup listing all risks in
  // that probability × severity bucket, with a "View Project" action.
  const [matrixCellRisks, setMatrixCellRisks] = useState<RiskSummaryDto[]>([]);
  const [matrixCellLabel, setMatrixCellLabel] = useState('');
  const [showNewRiskModal, setShowNewRiskModal] = useState(false);
  const [newRiskDraft, setNewRiskDraft] = useState<NewRiskDraft>(emptyRiskDraft);

  useEffect(() => {
    let cancelled = false;
    Promise.all([risksApi.getRisks({ page: 1, pageSize: 1000 }), risksApi.getKpis(), riskMatrixApi.getMatrix()])
      .then(([risksResp, kpisData, matrixData]) => {
        if (cancelled) return;
        setRisks(risksResp.data);
        setKpis(kpisData);
        setMatrix(matrixData);
        setLoading(false);
      })
      .catch((err) => {
        if (cancelled) return;
        setError(err instanceof Error ? err.message : 'Failed to load risks');
        setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const filteredRisks = useMemo(() => {
    const q = debouncedSearch.trim().toLowerCase();
    return risks.filter((r) => {
      const matchesSearch =
        !q ||
        r.title.toLowerCase().includes(q) ||
        r.number.toLowerCase().includes(q) ||
        r.projectCode.toLowerCase().includes(q);
      const matchesSeverity = severity === 'All' || r.severity === severity;
      const matchesStatus = status === 'All' || r.status === status;
      return matchesSearch && matchesSeverity && matchesStatus;
    });
  }, [risks, debouncedSearch, severity, status]);

  const kpiSummary = useMemo(() => {
    if (!kpis) return null;
    return [
      { key: 'critical' as const, label: 'Critical', value: kpis.critical },
      { key: 'high' as const, label: 'High', value: kpis.high },
      { key: 'medium' as const, label: 'Medium', value: kpis.medium },
      { key: 'mitigated' as const, label: 'Mitigated this month', value: kpis.mitigatedThisMonth },
    ];
  }, [kpis]);

  function handleMatrixCellClick(probability: RiskProbability, severity: RiskSeverity): void {
    // Look up the full RiskSummaryDto records for this cell using the risk
    // numbers from the matrix cell view model.
    const cell = matrix.find((c) => c.probability === probability && c.severity === severity);
    if (!cell || cell.riskIds.length === 0) return;
    const cellRisks = cell.riskIds
      .map((num) => risks.find((r) => r.number === num))
      .filter((r): r is RiskSummaryDto => r !== undefined);
    if (cellRisks.length === 0) return;
    setMatrixCellLabel(`${probability} Probability · ${severity} Impact`);
    setMatrixCellRisks(cellRisks);
  }

  function openNewRiskModal(): void {
    setNewRiskDraft(emptyRiskDraft());
    setShowNewRiskModal(true);
  }

  function handleSaveNewRisk(): void {
    if (!newRiskDraft.title.trim()) return;
    const nextId = risks.length ? Math.max(...risks.map((r) => r.id)) + 1 : 1;
    const impactDays = newRiskDraft.impactDays ? Number(newRiskDraft.impactDays) : undefined;
    const impactCost = newRiskDraft.impactCost ? Number(newRiskDraft.impactCost) : undefined;
    const draft: RiskSummaryDto = {
      id: nextId,
      projectId: Number(newRiskDraft.projectId) || 0,
      projectCode: newRiskDraft.projectCode.trim() || 'TBD',
      number: `RISK-${String(nextId).padStart(4, '0')}`,
      title: newRiskDraft.title.trim(),
      description: newRiskDraft.description.trim() || undefined,
      severity: newRiskDraft.severity,
      probability: newRiskDraft.probability,
      impactCost,
      impactDays,
      owner: newRiskDraft.owner.trim() || undefined,
      status: 'Open',
      mitigationPlan: newRiskDraft.mitigationPlan.trim() || undefined,
      identifiedDate: new Date().toISOString(),
      targetResolutionDate: newRiskDraft.targetResolutionDate || undefined,
      impactDisplay: '',
    };
    draft.impactDisplay = formatImpact(draft);
    // Demo only: kept in local component state so it's visible in the UI immediately;
    // nothing is written back to the API.
    setRisks((prev) => [draft, ...prev]);
    setSeverityFilter('All');
    setStatusFilter('All');
    setSearch('');
    setShowNewRiskModal(false);
  }

  function handleExportRisks(): void {
    downloadCsv<RiskSummaryDto>(
      'risks',
      [
        { header: 'ID', value: (r) => r.number },
        { header: 'Risk / Issue', value: (r) => r.title },
        // { header: 'Project', value: (r) => r.projectCode },
        { header: 'Severity', value: (r) => r.severity },
        { header: 'Probability', value: (r) => r.probability },
        { header: 'Impact', value: (r) => formatImpact(r) },
        { header: 'Owner', value: (r) => r.owner ?? '' },
        { header: 'Status', value: (r) => r.status },
      ],
      filteredRisks,
    );
  }

  return (
    <div className="risks-page">


      {loading && <div className="loading-state" aria-live="polite">Loading risks…</div>}
      {error && <div className="alert alert-error" role="alert">{error}</div>}

      {!loading && !error && (
        <>
          <div className="toolbar">
            <div className="toolbar-left">
              <div className="input-with-icon">
                <Icon className="icon" name="search" size={16} />
                <input
                  type="search"
                  className="input"
                  placeholder="Search risks…"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
              <select
                className="select"
                value={severity}
                onChange={(e) => setSeverityFilter(e.target.value as RiskSeverity | 'All')}
                aria-label="Filter risks by severity"
              >
                {severityOptions.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
              <select
                className="select"
                value={status}
                onChange={(e) => setStatusFilter(e.target.value as RiskStatus | 'All')}
                aria-label="Filter risks by status"
              >
                {statusOptions.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>
            <div className="toolbar-right">
              <button type="button" className="btn btn-secondary btn-sm" onClick={handleExportRisks} disabled={filteredRisks.length === 0}>
                <Icon className="icon" name="download" size={14} />
                Export
              </button>
              <button type="button" className="btn btn-primary" onClick={openNewRiskModal}>
                <Icon className="icon" name="plus" size={14} />
                New Risk
              </button>
            </div>
          </div>

          <div className="kpi-grid" style={{ marginBottom: 'var(--space-xl)' }}>
            {kpiSummary?.map((kpi) => (
              <div
                className="kpi-card"
                key={kpi.key}
                style={{ borderLeft: `4px solid ${kpiBorderColor[kpi.key]}` }}
              >
                <div className="kpi-label">{kpi.label}</div>
                <div className={`kpi-value ${kpi.key === 'critical' ? 'text-error' : kpi.key === 'mitigated' ? 'positive' : ''}`}>{kpi.value}</div>
                <div className={`kpi-change ${kpiChangeTone[kpi.key]}`}>
                  <Icon className="icon" name={kpiIcon[kpi.key]} size={14} />
                  {kpiChangeLabel[kpi.key]}
                </div>
              </div>
            ))}
          </div>

          <div className="card grid-card">
            <div className="card-header">
              <div>
                <h2 className="card-title">Risk Register</h2>
                <p className="card-subtitle">Open items across portfolio</p>
              </div>
            </div>
            <GridComponent
              ref={risksGridRef}
              dataSource={filteredRisks}
              allowPaging={true}
              allowSorting={true}
              pageSettings={{ pageSize: 20, pageCount: 4 }}
              gridLines="Horizontal"
              rowSelected={(args) => { if (args.data) setSelectedRisk(args.data as RiskSummaryDto); }}
            >
              <ColumnsDirective>
                <ColumnDirective
                  field="number"
                  headerText="ID"
                  width="80"
                  template={(r: RiskSummaryDto) => <span className="font-mono">{r.number}</span>}
                />
                <ColumnDirective
                  field="title"
                  headerText="Risk / Issue"
                  template={(r: RiskSummaryDto) => (
                    <span style={{ fontWeight: 600, whiteSpace: 'normal', wordBreak: 'break-word', display: 'block' }}>
                      {r.title}
                    </span>
                  )}
                />
                <ColumnDirective
                  field="severity"
                  headerText="Severity"
                  width="110"
                  template={(r: RiskSummaryDto) => <span className={`badge ${severityBadgeClass[r.severity]}`}>{r.severity}</span>}
                />
                <ColumnDirective field="probability" headerText="Probability" width="110" />
                <ColumnDirective
                  field="impactDisplay"
                  headerText="Impact"
                  width="90"
                  template={(r: RiskSummaryDto) => <span>{formatImpact(r)}</span>}
                />
                <ColumnDirective
                  field="owner"
                  headerText="Owner"
                  width="150"
                  template={(r: RiskSummaryDto) => <span>{r.owner || '—'}</span>}
                />
                <ColumnDirective
                  field="status"
                  headerText="Mitigation Status"
                  width="150"
                  template={(r: RiskSummaryDto) => <span className={`badge ${statusBadgeClass[r.status]}`}>{r.status}</span>}
                />
              </ColumnsDirective>
              <Inject services={[Page, Sort, Resize]} />
            </GridComponent>
          </div>

          <div className="card risk-matrix-card">
            <div className="card-header">
              <div>
                <h2 className="card-title">Risk Matrix</h2>
                <p className="card-subtitle">Probability × Impact</p>
              </div>
            </div>
            <RiskMatrixHeatmap matrix={matrix} onCellClick={handleMatrixCellClick} />
          </div>
        </>
      )}

      <Modal
        open={!!selectedRisk}
        onClose={() => setSelectedRisk(null)}
        title={selectedRisk ? selectedRisk.title : ''}
        subtitle={selectedRisk ? `${selectedRisk.number} · ${selectedRisk.projectCode}` : undefined}
        size="lg"
        footer={
          selectedRisk && (
            <>
              <button type="button" className="btn btn-secondary" onClick={() => setSelectedRisk(null)}>
                Close
              </button>
              <button
                type="button"
                className="btn btn-primary"
                onClick={() => {
                  const projectId = selectedRisk.projectId;
                  setSelectedRisk(null);
                  navigate(`/projects/${projectId}`);
                }}
              >
                <Icon className="icon" name="external-link" size={14} />
                View Project Details
              </button>
            </>
          )
        }
      >
        {selectedRisk && (
          <div className="modal-detail-grid">
            <div className="modal-detail-item">
              <span className="modal-detail-label">Severity</span>
              <span className={`badge ${severityBadgeClass[selectedRisk.severity]}`} style={{ width: 'fit-content' }}>
                {selectedRisk.severity}
              </span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Status</span>
              <span className={`badge ${statusBadgeClass[selectedRisk.status]}`} style={{ width: 'fit-content' }}>
                {selectedRisk.status}
              </span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Probability</span>
              <span className="modal-detail-value">{selectedRisk.probability}</span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Impact</span>
              <span className="modal-detail-value">{formatImpact(selectedRisk)}</span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Owner</span>
              <span className="modal-detail-value">{selectedRisk.owner || '—'}</span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Identified</span>
              <span className="modal-detail-value">{formatDate(selectedRisk.identifiedDate)}</span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Target Resolution</span>
              <span className="modal-detail-value">
                {selectedRisk.targetResolutionDate ? formatDate(selectedRisk.targetResolutionDate) : '—'}
              </span>
            </div>
            <div className="modal-detail-item">
              <span className="modal-detail-label">Closed</span>
              <span className="modal-detail-value">{selectedRisk.closedDate ? formatDate(selectedRisk.closedDate) : '—'}</span>
            </div>
            {selectedRisk.description && (
              <div className="modal-detail-item is-span-2">
                <span className="modal-detail-label">Description</span>
                <span className="modal-detail-value" style={{ fontWeight: 400 }}>{selectedRisk.description}</span>
              </div>
            )}
            {selectedRisk.mitigationPlan && (
              <div className="modal-detail-item is-span-2">
                <span className="modal-detail-label">Mitigation Plan</span>
                <span className="modal-detail-value" style={{ fontWeight: 400 }}>{selectedRisk.mitigationPlan}</span>
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* ── Heatmap cell drill-down modal ───────────────────────────────── */}
      <Modal
        open={matrixCellRisks.length > 0}
        onClose={() => setMatrixCellRisks([])}
        title="Risks in this cell"
        subtitle={matrixCellLabel}
        size="lg"
        footer={
          <button type="button" className="btn btn-secondary" onClick={() => setMatrixCellRisks([])}>
            Close
          </button>
        }
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-sm)' }}>
          {matrixCellRisks.map((r) => (
            <div
              key={r.id}
              className="card"
              style={{ padding: 'var(--space-md)', cursor: 'pointer', borderLeft: `4px solid var(--color-${r.severity === 'Critical' || r.severity === 'High' ? 'error' : r.severity === 'Medium' ? 'warning' : 'success'})` }}
              role="button"
              tabIndex={0}
              onClick={() => { setMatrixCellRisks([]); setSelectedRisk(r); }}
              onKeyDown={onActivateKey(() => { setMatrixCellRisks([]); setSelectedRisk(r); })}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 'var(--space-sm)' }}>
                <div>
                  <span className="font-mono" style={{ fontSize: 'var(--text-small-size)', color: 'var(--color-secondary)' }}>{r.number}</span>
                  <p style={{ fontWeight: 600, margin: '2px 0 4px' }}>{r.title}</p>
                  <span style={{ fontSize: 'var(--text-small-size)', color: 'var(--color-secondary)' }}>{r.projectCode} · {r.owner || 'Unassigned'}</span>
                </div>
                <div style={{ display: 'flex', gap: 'var(--space-xs)', flexShrink: 0 }}>
                  <span className={`badge ${severityBadgeClass[r.severity]}`}>{r.severity}</span>
                  <span className={`badge ${statusBadgeClass[r.status]}`}>{r.status}</span>
                </div>
              </div>
              <div style={{ marginTop: 'var(--space-sm)', display: 'flex', gap: 'var(--space-sm)', alignItems: 'center', justifyContent: 'space-between' }}>
                <span style={{ fontSize: 'var(--text-small-size)', color: 'var(--color-secondary)' }}>Impact: {formatImpact(r)}</span>
                <button
                  type="button"
                  className="btn btn-primary btn-sm"
                  onClick={(e) => { e.stopPropagation(); setMatrixCellRisks([]); navigate(`/projects/${r.projectId}`); }}
                >
                  <i className="icon icon-external-link" aria-hidden="true" />
                  View Project
                </button>
              </div>
            </div>
          ))}
        </div>
      </Modal>

      <Modal
        open={showNewRiskModal}
        onClose={() => setShowNewRiskModal(false)}
        title="New Risk"
        subtitle="Demo template — saved to this view only; nothing is sent to the backend."
        size="lg"
        footer={
          <>
            <button type="button" className="btn btn-secondary" onClick={() => setShowNewRiskModal(false)}>
              Cancel
            </button>
            <button
              type="button"
              className="btn btn-primary"
              onClick={handleSaveNewRisk}
              disabled={!newRiskDraft.title.trim()}
            >
              Save Risk
            </button>
          </>
        }
      >
        <div className="modal-form-grid">
          <div className="modal-form-field is-span-2">
            <label htmlFor="risk-title">Title</label>
            <input
              id="risk-title"
              className="input"
              type="text"
              value={newRiskDraft.title}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, title: e.target.value }))}
              required
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-project-id">Project ID</label>
            <input
              id="risk-project-id"
              className="input"
              type="number"
              value={newRiskDraft.projectId}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, projectId: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-project-code">Project Code</label>
            <input
              id="risk-project-code"
              className="input"
              type="text"
              value={newRiskDraft.projectCode}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, projectCode: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-severity">Severity</label>
            <select
              id="risk-severity"
              className="select"
              value={newRiskDraft.severity}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, severity: e.target.value as RiskSeverity }))}
            >
              {severities.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-probability">Probability</label>
            <select
              id="risk-probability"
              className="select"
              value={newRiskDraft.probability}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, probability: e.target.value as RiskProbability }))}
            >
              {probabilities.map((p) => (
                <option key={p} value={p}>{p}</option>
              ))}
            </select>
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-owner">Owner</label>
            <input
              id="risk-owner"
              className="input"
              type="text"
              value={newRiskDraft.owner}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, owner: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-target-date">Target Resolution</label>
            <input
              id="risk-target-date"
              className="input"
              type="date"
              value={newRiskDraft.targetResolutionDate}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, targetResolutionDate: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-impact-days">Schedule Impact (days)</label>
            <input
              id="risk-impact-days"
              className="input"
              type="number"
              value={newRiskDraft.impactDays}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, impactDays: e.target.value }))}
            />
          </div>
          <div className="modal-form-field">
            <label htmlFor="risk-impact-cost">Cost Impact ($)</label>
            <input
              id="risk-impact-cost"
              className="input"
              type="number"
              value={newRiskDraft.impactCost}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, impactCost: e.target.value }))}
            />
          </div>
          <div className="modal-form-field is-span-2">
            <label htmlFor="risk-description">Description</label>
            <textarea
              id="risk-description"
              className="input"
              rows={2}
              value={newRiskDraft.description}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, description: e.target.value }))}
            />
          </div>
          <div className="modal-form-field is-span-2">
            <label htmlFor="risk-mitigation">Mitigation Plan</label>
            <textarea
              id="risk-mitigation"
              className="input"
              rows={3}
              value={newRiskDraft.mitigationPlan}
              onChange={(e) => setNewRiskDraft((d) => ({ ...d, mitigationPlan: e.target.value }))}
            />
          </div>
        </div>
      </Modal>
    </div>
  );
}
