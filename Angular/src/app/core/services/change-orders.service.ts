import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type { ChangeOrderSummaryDto, PagedResponse, QueryParameters } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ChangeOrdersService {
  private api = inject(ApiClientService);

  getChangeOrders(params?: QueryParameters): Observable<PagedResponse<ChangeOrderSummaryDto>> {
    return this.api.getJson<PagedResponse<ChangeOrderSummaryDto>>('changeorders', params);
  }
}
