import { Routes } from '@angular/router';
import { Shell } from './layout/shell/shell';
import { Dashboard } from './pages/dashboard/dashboard';
import { Projects } from './pages/projects/projects';
import { ProjectDetail } from './pages/project-detail/project-detail';
import { CostControl } from './pages/cost-control/cost-control';
import { Risks } from './pages/risks/risks';
import { NotFound } from './pages/not-found/not-found';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    children: [
      { path: '', component: Dashboard },
      { path: 'projects', component: Projects },
      { path: 'projects/:id', component: ProjectDetail },
      { path: 'cost-control', component: CostControl },
      { path: 'risks', component: Risks },
      { path: '**', component: NotFound },
    ],
  },
];
