export interface NavigationItem {
  id: string;
  label: string;
  path: string;
  icon: string;
  priority: 'P0' | 'P1' | 'P2';
}

export const primaryNav: NavigationItem[] = [
  { id: 'dashboard', label: 'Dashboard', path: '/', icon: 'layout-dashboard', priority: 'P0' },
  { id: 'projects', label: 'Projects', path: '/projects', icon: 'briefcase', priority: 'P0' },
  { id: 'cost-control', label: 'Cost Control', path: '/cost-control', icon: 'wallet', priority: 'P0' },
  { id: 'risks', label: 'Risks & Issues', path: '/risks', icon: 'alert-triangle', priority: 'P0' },
];

export const secondaryNav: NavigationItem[] = [
  { id: 'reports', label: 'Reports', path: '/reports', icon: 'bar-chart-3', priority: 'P2' },
];
