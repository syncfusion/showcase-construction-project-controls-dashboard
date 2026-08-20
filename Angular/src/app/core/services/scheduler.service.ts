import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type { CalendarEventDto, QueryParameters } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class SchedulerService {
  private api = inject(ApiClientService);

  getEvents(params?: QueryParameters): Observable<CalendarEventDto[]> {
    return this.api.getJson<CalendarEventDto[]>('scheduler/appointments', params).pipe(
      map((events) =>
        events.map((e) => ({
          ...e,
          startTime: new Date(e.startTime).toISOString(),
          endTime: new Date(e.endTime).toISOString(),
        })),
      ),
    );
  }
}
