import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type {
  ChangeOrderSummaryDto,
  MapLocationDto,
  PagedResponse,
  ProjectDetailDto,
  ProjectKpisDto,
  ProjectMilestoneDto,
  ProjectSummaryDto,
  QueryParameters,
  RecentDocumentDto,
  RfiSummaryDto,
  RiskSummaryDto,
  SubmittalSummaryDto,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private api = inject(ApiClientService);

  getProjects(params?: QueryParameters): Observable<PagedResponse<ProjectSummaryDto>> {
    return this.api.getJson<PagedResponse<ProjectSummaryDto>>('projects', params);
  }

  getById(id: number): Observable<ProjectDetailDto> {
    return this.api.getJson<ProjectDetailDto>(`projects/${id}`);
  }

  getKpis(id: number): Observable<ProjectKpisDto> {
    return this.api.getJson<ProjectKpisDto>(`projects/${id}/kpis`);
  }

  getTopRisks(id: number, limit = 5): Observable<RiskSummaryDto[]> {
    return this.api.getJson<RiskSummaryDto[]>(`projects/${id}/top-open-risks`, { limit });
  }

  // NOTE: returns ProjectMilestoneDto[], NOT UpcomingMilestoneDto[] — a different shape
  // (plannedDate/status/owner vs dueDate/healthStatus) from the portfolio-wide report endpoint.
  getUpcomingMilestones(id: number, days = 60, limit = 10): Observable<ProjectMilestoneDto[]> {
    return this.api.getJson<ProjectMilestoneDto[]>(`projects/${id}/upcoming-milestones`, { days, limit });
  }

  getRecentDocuments(id: number, days = 90, limit = 10): Observable<RecentDocumentDto[]> {
    return this.api.getJson<RecentDocumentDto[]>(`projects/${id}/recent-documents`, { days, limit });
  }

  getRfis(id: number, limit = 50): Observable<RfiSummaryDto[]> {
    return this.api.getJson<RfiSummaryDto[]>(`projects/${id}/rfis`, { limit });
  }

  getSubmittals(id: number, limit = 50): Observable<SubmittalSummaryDto[]> {
    return this.api.getJson<SubmittalSummaryDto[]>(`projects/${id}/submittals`, { limit });
  }

  getChangeOrders(id: number, limit = 50): Observable<ChangeOrderSummaryDto[]> {
    return this.api.getJson<ChangeOrderSummaryDto[]>(`projects/${id}/change-orders`, { limit });
  }

  getLocations(): Observable<MapLocationDto[]> {
    return this.api.getJson<MapLocationDto[]>('projects/locations');
  }
}
