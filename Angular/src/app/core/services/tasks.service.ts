import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type { GanttTask, PagedResponse, QueryParameters, TaskDto } from '../models/api.models';

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

@Injectable({ providedIn: 'root' })
export class TasksService {
  private api = inject(ApiClientService);

  getTasks(params?: QueryParameters): Observable<PagedResponse<GanttTask>> {
    return this.api.getJson<PagedResponse<TaskDto>>('tasks', params).pipe(
      map((response) => ({
        ...response,
        data: response.data.map((t) => mapTaskToGantt(t, `Project ${t.projectId}`)),
      })),
    );
  }
}
