import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type {
  CostKpisDto,
  CostPerformancePointDto,
  CostVarianceByCostCodeDto,
  EarnedValuePointDto,
  PortfolioKpisDto,
  ProjectHealthDistributionDto,
  UpcomingMilestoneDto,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private api = inject(ApiClientService);

  getPortfolioKpis(): Observable<PortfolioKpisDto> {
    return this.api.getJson<PortfolioKpisDto>('reports/portfolio-kpis');
  }

  getCostPerformanceTrend(months = 12): Observable<CostPerformancePointDto[]> {
    return this.api.getJson<CostPerformancePointDto[]>('reports/cost-performance-trend', { months });
  }

  getProjectHealthDistribution(): Observable<ProjectHealthDistributionDto> {
    return this.api.getJson<ProjectHealthDistributionDto>('reports/project-health-distribution');
  }

  getUpcomingMilestones(days = 30, limit = 20): Observable<UpcomingMilestoneDto[]> {
    return this.api.getJson<UpcomingMilestoneDto[]>('reports/upcoming-milestones', { days, limit });
  }

  getCostKpis(): Observable<CostKpisDto> {
    return this.api.getJson<CostKpisDto>('reports/cost-kpis');
  }

  getEarnedValueTrend(months = 12): Observable<EarnedValuePointDto[]> {
    return this.api.getJson<EarnedValuePointDto[]>('reports/earned-value-trend', { months });
  }

  getCostVarianceByCostCode(): Observable<CostVarianceByCostCodeDto[]> {
    return this.api.getJson<CostVarianceByCostCodeDto[]>('reports/cost-variance-by-cost-code');
  }
}
