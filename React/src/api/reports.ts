import type {
  CostKpisDto,
  CostPerformancePointDto,
  CostVarianceByCostCodeDto,
  EarnedValuePointDto,
  PortfolioKpisDto,
  ProjectHealthDistributionDto,
  UpcomingMilestoneDto,
  ProjectSummaryDto,
  ProjectDetailDto,
  ProjectKpisDto,
  TaskDto,
  GanttTask,
  RiskSummaryDto,
  RiskUpdateClientDto,
  RiskKpisDto,
  RiskMatrixDto,
  RiskMatrixCellViewModel,
  CalendarEventDto,
  MapLocationDto,
  RecentDocumentDto,
  ChangeOrderSummaryDto,
  RfiSummaryDto,
  SubmittalSummaryDto,
  ProjectMilestoneDto,
  QueryParameters,
} from '../types';
import { getJson, postJson, putJson, deleteJson } from './client';

export const reportsApi = {
  getPortfolioKpis: () => getJson<PortfolioKpisDto>('reports/portfolio-kpis'),
  getCostPerformanceTrend: (months = 12) =>
    getJson<CostPerformancePointDto[]>('reports/cost-performance-trend', { months }),
  getProjectHealthDistribution: () => getJson<ProjectHealthDistributionDto>('reports/project-health-distribution'),
  getUpcomingMilestones: (days = 30, limit = 20) =>
    getJson<UpcomingMilestoneDto[]>('reports/upcoming-milestones', { days, limit }),
  getCostKpis: () => getJson<CostKpisDto>('reports/cost-kpis'),
  getEarnedValueTrend: (months = 12) =>
    getJson<EarnedValuePointDto[]>('reports/earned-value-trend', { months }),
  getCostVarianceByCostCode: () => getJson<CostVarianceByCostCodeDto[]>('reports/cost-variance-by-cost-code'),
};

function mapTaskToGantt(t: TaskDto, projectName: string): GanttTask {
  const start = new Date(t.startDate);
  const end = new Date(t.endDate);
  const duration = Math.max(1, Math.round((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)));
  return {
    TaskID: t.id,
    TaskName: t.name,
    StartDate: start,
    EndDate: end,
    Duration: duration,
    Progress: t.progress,
    ParentID: t.parentTaskId ?? null,
    Status: t.status,
    ProjectName: projectName,
    AssignedTo: t.assignedTo ?? null,
  };
}

export const projectsApi = {
  getProjects: (params?: QueryParameters) =>
    getJson<{ data: ProjectSummaryDto[]; totalCount: number; page: number; pageSize: number; totalPages: number }>('projects', params),
  getById: (id: number) => getJson<ProjectDetailDto>(`projects/${id}`),
  getKpis: (id: number) => getJson<ProjectKpisDto>(`projects/${id}/kpis`),
  getTopRisks: (id: number, limit = 5) =>
    getJson<RiskSummaryDto[]>(`projects/${id}/top-open-risks`, { limit }),
  getUpcomingMilestones: (id: number, days = 60, limit = 10) =>
    getJson<ProjectMilestoneDto[]>(`projects/${id}/upcoming-milestones`, { days, limit }),
  getRecentDocuments: (id: number, days = 90, limit = 10) =>
    getJson<RecentDocumentDto[]>(`projects/${id}/recent-documents`, { days, limit }),
  getRfis: (id: number, limit = 50) =>
    getJson<RfiSummaryDto[]>(`projects/${id}/rfis`, { limit }),
  getSubmittals: (id: number, limit = 50) =>
    getJson<SubmittalSummaryDto[]>(`projects/${id}/submittals`, { limit }),
  getChangeOrders: (id: number, limit = 50) =>
    getJson<ChangeOrderSummaryDto[]>(`projects/${id}/change-orders`, { limit }),
  getLocations: () => getJson<MapLocationDto[]>('projects/locations'),
};

export const tasksApi = {
  getTasks: async (params?: QueryParameters) => {
    const response = await getJson<{ data: TaskDto[]; totalCount: number; page: number; pageSize: number; totalPages: number }>('tasks', params);
    return {
      ...response,
      data: response.data.map((t) => mapTaskToGantt(t, `Project ${t.projectId}`)),
    };
  },
};

export const risksApi = {
  getRisks: (params?: QueryParameters) =>
    getJson<{ data: RiskSummaryDto[]; totalCount: number; page: number; pageSize: number; totalPages: number }>('risks', params),
  getKpis: () => getJson<RiskKpisDto>('risks/kpis'),
  update: (id: number, changes: RiskUpdateClientDto) =>
    putJson<RiskSummaryDto>(`risks/${id}`, changes),
  create: (risk: Omit<RiskSummaryDto, 'id' | 'projectCode' | 'impactDisplay'>) =>
    postJson<RiskSummaryDto>('risks', risk),
  delete: (id: number) => deleteJson<void>(`risks/${id}`),
};

export const schedulerApi = {
  getEvents: (params?: QueryParameters) =>
    getJson<CalendarEventDto[]>('scheduler/appointments', params).then((events) =>
      events.map((e) => ({
        ...e,
        startTime: new Date(e.startTime).toISOString(),
        endTime: new Date(e.endTime).toISOString(),
      }))
    ),
};

export const changeOrdersApi = {
  getChangeOrders: (params?: QueryParameters) =>
    getJson<{ data: ChangeOrderSummaryDto[]; totalCount: number; page: number; pageSize: number; totalPages: number }>('changeorders', params),
};

function flattenMatrix(matrix: RiskMatrixDto): RiskMatrixCellViewModel[] {
  return matrix.rows.flatMap((row) =>
    row.cells.map((cell) => ({
      probability: row.probability,
      severity: cell.severity,
      count: cell.riskNumbers.length,
      riskIds: cell.riskNumbers,
    }))
  );
}

export const riskMatrixApi = {
  getMatrix: () => getJson<RiskMatrixDto>('risks/matrix').then(flattenMatrix),
};
