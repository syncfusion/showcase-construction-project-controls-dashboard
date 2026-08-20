import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type {
  PagedResponse,
  QueryParameters,
  RiskKpisDto,
  RiskSummaryDto,
  RiskUpdateClientDto,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class RisksService {
  private api = inject(ApiClientService);

  getRisks(params?: QueryParameters): Observable<PagedResponse<RiskSummaryDto>> {
    return this.api.getJson<PagedResponse<RiskSummaryDto>>('risks', params);
  }

  getKpis(): Observable<RiskKpisDto> {
    return this.api.getJson<RiskKpisDto>('risks/kpis');
  }

  update(id: number, changes: RiskUpdateClientDto): Observable<RiskSummaryDto> {
    return this.api.putJson<RiskSummaryDto>(`risks/${id}`, changes);
  }

  // Kept for API-surface parity with the React app; intentionally NOT called by the
  // Risks page's "New Risk" modal, which stays local-state-only (demo data, no persistence).
  create(risk: Omit<RiskSummaryDto, 'id' | 'projectCode' | 'impactDisplay'>): Observable<RiskSummaryDto> {
    return this.api.postJson<RiskSummaryDto>('risks', risk);
  }

  delete(id: number): Observable<void> {
    return this.api.deleteJson<void>(`risks/${id}`);
  }
}
