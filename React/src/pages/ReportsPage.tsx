import type { ReactElement } from 'react';
import { useEffect, useMemo, useState } from 'react';
import {
  Category,
  ChartComponent,
  ColumnSeries,
  DataLabel,
  Inject,
  Legend,
  LineSeries,
  SeriesCollectionDirective,
  SeriesDirective,
  Tooltip,
} from '@syncfusion/ej2-react-charts';
import { reportsApi } from '../api/reports';
import type { CostVarianceByCostCodeDto, EarnedValuePointDto } from '../types';
import './ReportsPage.css';

export function ReportsPage(): ReactElement {
  const [earnedValueTrend, setEarnedValueTrend] = useState<EarnedValuePointDto[]>([]);
  const [costVariance, setCostVariance] = useState<CostVarianceByCostCodeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    Promise.all([
      reportsApi.getEarnedValueTrend(12),
      reportsApi.getCostVarianceByCostCode(),
    ])
      .then(([evData, cvData]) => {
        if (!cancelled) {
          setEarnedValueTrend(evData);
          setCostVariance(cvData);
          setLoading(false);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Failed to load reports');
          setLoading(false);
        }
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const evData = useMemo(
    () =>
      earnedValueTrend.map((p) => ({
        month: p.month,
        bcws: p.bcws,
        bcwp: p.bcwp,
        acwp: p.acwp,
      })),
    [earnedValueTrend]
  );

  const cvData = useMemo(
    () =>
      costVariance.map((c) => ({
        costCode: c.costCode,
        variance: c.variancePct,
      })),
    [costVariance]
  );

  return (
    <div className='reports-page'>
      <header className='page-header'>
        <h1>Reports</h1>
        <p>Historical trends, filters, and export-ready dashboards</p>
      </header>

      {loading && <div className='loading-state' aria-live='polite'>Loading reports…</div>}
      {error && <div className='alert alert-error' role='alert'>{error}</div>}

      {!loading && !error && (
        <div className='reports-grid'>
          <div className='card chart-card'>
            <h2 className='card-title'>Earned Value Trend</h2>
            <ChartComponent
              id='earned-value-chart'
              primaryXAxis={{ valueType: 'Category', title: 'Month' }}
              primaryYAxis={{ title: 'Value ($)', labelFormat: '${value}' }}
              tooltip={{ enable: true }}
              legendSettings={{ visible: true, position: 'Bottom' }}
              height='360px'
            >
              <Inject services={[LineSeries, Category, Legend, Tooltip, DataLabel]} />
              <SeriesCollectionDirective>
                <SeriesDirective
                  dataSource={evData}
                  xName='month'
                  yName='bcws'
                  name='Planned Value (BCWS)'
                  type='Line'
                  width={2}
                />
                <SeriesDirective
                  dataSource={evData}
                  xName='month'
                  yName='bcwp'
                  name='Earned Value (BCWP)'
                  type='Line'
                  width={2}
                />
                <SeriesDirective
                  dataSource={evData}
                  xName='month'
                  yName='acwp'
                  name='Actual Cost (ACWP)'
                  type='Line'
                  width={2}
                />
              </SeriesCollectionDirective>
            </ChartComponent>
          </div>

          <div className='card chart-card'>
            <h2 className='card-title'>Cost Variance by Cost Code</h2>
            <ChartComponent
              id='cost-variance-chart'
              primaryXAxis={{ valueType: 'Category', title: 'Cost Code' }}
              primaryYAxis={{ title: 'Variance (%)', labelFormat: '{value}%' }}
              tooltip={{ enable: true }}
              height='360px'
            >
              <Inject services={[ColumnSeries, Category, Tooltip, DataLabel]} />
              <SeriesCollectionDirective>
                <SeriesDirective
                  dataSource={cvData}
                  xName='costCode'
                  yName='variance'
                  name='Variance %'
                  type='Column'
                />
              </SeriesCollectionDirective>
            </ChartComponent>
          </div>
        </div>
      )}
    </div>
  );
}
