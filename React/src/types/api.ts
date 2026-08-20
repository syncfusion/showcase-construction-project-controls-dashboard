export type HealthStatus = 'NotStarted' | 'OnTrack' | 'AtRisk' | 'Critical';
export type ProjectStatus = 'Planning' | 'Active' | 'OnHold' | 'Completed' | 'Cancelled';
export type TaskStatus = 'NotStarted' | 'InProgress' | 'OnHold' | 'Completed' | 'Cancelled';
export type RiskSeverity = 'Low' | 'Medium' | 'High' | 'Critical';
export type RiskProbability = 'Low' | 'Medium' | 'High';
export type RiskStatus = 'Open' | 'InProgress' | 'Monitoring' | 'Escalated' | 'Containment' | 'Mitigated' | 'Closed';
export type ChangeOrderStatus = 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected';

export interface QueryParameters extends Record<string, string | number | boolean | undefined> {
  search?: string;
  page?: number;
  pageSize?: number;
  sort?: string;
  filter?: string;
}

export interface PortfolioKpisDto {
  activeProjects: number;
  scheduleVariancePct: number;
  costVariancePct: number;
  cpi: number;
  spi: number;
  openRisks: number;
  criticalRisks: number;
}

export interface CostKpisDto {
  totalPortfolioBudget: number;
  committedSpend: number;
  forecastAtCompletion: number;
  pendingChangeOrdersAmount: number;
  pendingChangeOrdersCount: number;
}

export interface ProjectHealthDistributionDto {
  onTrack: number;
  atRisk: number;
  critical: number;
  notStarted: number;
  total: number;
}

export interface UpcomingMilestoneDto {
  projectCode: string;
  projectName: string;
  title: string;
  dueDate: string;
  owner: string | null;
  healthStatus: HealthStatus;
}

// Shape returned by GET /projects/{id}/upcoming-milestones — distinct from
// UpcomingMilestoneDto above (portfolio-wide endpoint): dates/status/owner use
// different field names and the milestone carries its own lifecycle status
// rather than the parent project's health.
export interface ProjectMilestoneDto {
  id: number;
  projectId: number;
  projectCode: string;
  title: string;
  description?: string;
  plannedDate: string;
  status: TaskStatus;
  owner?: string | null;
  floatDays: number;
  type?: string | null;
}

export interface CostPerformancePointDto {
  month: string;
  planned: number;
  actual: number;
}

export interface EarnedValuePointDto {
  month: string;
  bcws: number;
  bcwp: number;
  acwp: number;
}

export interface CostVarianceByCostCodeDto {
  costCode: string;
  variancePct: number;
}

export interface ProjectSummaryDto {
  id: number;
  name: string;
  code: string;
  description?: string;
  startDate: string;
  endDate: string;
  status: ProjectStatus;
  location?: string;
  budget: number;
  progress: number;
  manager?: string;
  createdDate: string;
  modifiedDate?: string;
  healthStatus: HealthStatus;
}

export interface ProjectDetailDto extends ProjectSummaryDto {
  // identical shape to backend ProjectDto; empty interface preserves naming consistency
}

export interface ProjectKpisDto {
  percentComplete: number;
  costVariance: number;
  scheduleVariance: number;
  openRfis: number;
  overdueRfis: number;
  budget: number;
  spent: number;
  openChangeOrders: number;
  openRisks: number;
  openSubmittals: number;
  overdueSubmittals: number;
  approvedSubmittals: number;
  pendingChangeOrdersAmount: number;
}

export interface RfiSummaryDto {
  id: number;
  projectId: number;
  number: string;
  subject: string;
  status: string;
  discipline?: string;
  impact?: string;
  requestedBy?: string;
  requestDate: string;
  dueDate?: string;
  answeredDate?: string;
}

export interface SubmittalSummaryDto {
  id: number;
  projectId: number;
  number: string;
  title: string;
  status: string;
  discipline?: string;
  specificationSection?: string;
  submittalType?: string;
  submittedBy?: string;
  submittedDate: string;
  requiredDate?: string;
  approvedDate?: string;
}

export interface TaskDto {
  id: number;
  projectId: number;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  status: TaskStatus;
  progress: number;
  assignedTo?: string;
  parentTaskId?: number;
  dependencies?: string;
  createdDate: string;
  modifiedDate?: string;
}

export interface GanttTask {
  TaskID: number;
  TaskName: string;
  StartDate: Date;
  EndDate: Date;
  Duration: number;
  Progress: number;
  ParentID?: number | null;
  Status: TaskStatus;
  ProjectName: string;
  AssignedTo: string | null;
}

export interface RiskSummaryDto {
  id: number;
  projectId: number;
  projectCode: string;
  number: string;
  title: string;
  description?: string;
  severity: RiskSeverity;
  probability: RiskProbability;
  impactType?: string;
  impactDescription?: string;
  impactCost?: number;
  impactDays?: number;
  owner?: string;
  status: RiskStatus;
  mitigationPlan?: string;
  identifiedDate: string;
  targetResolutionDate?: string;
  closedDate?: string;
  impactDisplay: string;
}

export interface RiskUpdateClientDto {
  status: RiskStatus;
  owner: string;
}

export interface RiskKpisDto {
  critical: number;
  high: number;
  medium: number;
  low: number;
  mitigatedThisMonth: number;
  open: number;
}

export interface RiskMatrixDto {
  rows: RiskMatrixRowDto[];
}

export interface RiskMatrixRowDto {
  probability: RiskProbability;
  cells: RiskMatrixServerCellDto[];
}

export interface CalendarEventDto {
  id: number;
  subject: string;
  startTime: string;
  endTime: string;
  location?: string;
  isAllDay: boolean;
  description?: string;
}

export interface MapLocationDto {
  projectId: number;
  latitude: number;
  longitude: number;
  name: string;
  status?: string;
  color?: string;
  startDate?: string;
  endDate?: string;
  progress: number;
  health?: HealthStatus;
  topIssues: string[];
}

export interface RecentDocumentDto {
  id: number;
  documentNumber: string;
  title: string;
  type: string;
  revision?: string;
  submittedDate: string;
  status: string;
  projectId: number;
}

export interface ChangeOrderSummaryDto {
  id: number;
  projectId: number;
  projectCode: string;
  number: string;
  description: string;
  amount: number;
  status: ChangeOrderStatus;
  requestedBy?: string;
  requestDate?: string;
  approvedBy?: string;
  approvalDate?: string;
  justification?: string;
  impactDays?: number;
  createdDate: string;
  modifiedDate?: string;
}

export interface RiskMatrixServerCellDto {
  severity: RiskSeverity;
  riskNumbers: string[];
}

export interface RiskMatrixCellViewModel {
  probability: RiskProbability;
  severity: RiskSeverity;
  count: number;
  riskIds: string[];
}
