import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ApiClientService } from './api-client.service';
import type { RiskMatrixCellViewModel, RiskMatrixDto } from '../models/api.models';

function flattenMatrix(matrix: RiskMatrixDto): RiskMatrixCellViewModel[] {
  return matrix.rows.flatMap((row) =>
    row.cells.map((cell) => ({
      probability: row.probability,
      severity: cell.severity,
      count: cell.riskNumbers.length,
      riskIds: cell.riskNumbers,
    })),
  );
}

@Injectable({ providedIn: 'root' })
export class RiskMatrixService {
  private api = inject(ApiClientService);

  getMatrix(): Observable<RiskMatrixCellViewModel[]> {
    return this.api.getJson<RiskMatrixDto>('risks/matrix').pipe(map(flattenMatrix));
  }
}
