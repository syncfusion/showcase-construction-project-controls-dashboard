import { createBrowserRouter } from 'react-router-dom';
import { Shell } from './layout/Shell';
import { CostControlPage, DashboardPage, NotFoundPage, ProjectDetailPage, ProjectsPage, ReportsPage, RisksPage } from './pages';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Shell />,
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'projects', element: <ProjectsPage /> },
      { path: 'projects/:id', element: <ProjectDetailPage /> },
      { path: 'cost-control', element: <CostControlPage /> },
      { path: 'risks', element: <RisksPage /> },
      { path: 'reports', element: <ReportsPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
]);
