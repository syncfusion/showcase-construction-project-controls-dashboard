import { type ReactNode, useState, useCallback, useEffect, useMemo } from 'react';
import { NavLink, Outlet, useLocation } from 'react-router-dom';
import type { NavigationItem } from './navigation';
import { primaryNav } from './navigation';
import { Icon } from '../components/Icon';
import './Shell.css';

interface PageMeta {
  title: string;
  subtitle: string;
}

// Per-route header metadata shown in the topbar. Dynamic routes (e.g.
// /projects/:id) intentionally have no entry here — those pages keep their
// in-content header, so the topbar remains clean for them.
const PAGE_META: Record<string, PageMeta> = {
  '/': { title: 'Dashboard', subtitle: 'Real-time project controls across the construction portfolio.' },
  '/projects': { title: 'Projects', subtitle: 'Portfolio overview of all active and planned construction projects.' },
  '/cost-control': { title: 'Cost Control', subtitle: 'Track budgets, committed spend, forecasts, and change orders.' },
  '/risks': { title: 'Risks & Issues', subtitle: 'Track, score, and mitigate project risks before they impact cost or schedule.' },
};

// Outlet context that lets a page render custom content into the topbar's
// page-title slot (used by the Project Detail page for Back + breadcrumb +
// project name). Pages opt in by calling setTopbarHeading(<... />).
export interface ShellOutletContext {
  setTopbarHeading: (node: ReactNode) => void;
}

const navItemClassName = ({ isActive }: { isActive: boolean }): string =>
  `nav-item${isActive ? ' is-active' : ''}`;

function NavIcon({ name }: { name: string }) {
  return <Icon className="icon" name={name} size={18} />;
}

function SidebarNav({ items, onNavigate }: { items: NavigationItem[]; onNavigate?: () => void }) {
  return (
    <ul className="nav-list">
      {items.map((item) => (
        <li key={item.id}>
          <NavLink to={item.path} className={navItemClassName} onClick={onNavigate}>
            <NavIcon name={item.icon} />
            <span>{item.label}</span>
          </NavLink>
        </li>
      ))}
    </ul>
  );
}

function ThemeToggle({ theme, onToggle }: { theme: 'light' | 'dark'; onToggle: () => void }) {
  return (
    <button
      type="button"
      className="btn btn-icon btn-ghost"
      onClick={onToggle}
      aria-label={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
      title={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
    >
      <NavIcon name={theme === 'light' ? 'moon' : 'sun'} />
    </button>
  );
}

export function Shell(): ReactNode {
  const location = useLocation();
  const pageMeta = useMemo<PageMeta | null>(() => PAGE_META[location.pathname] ?? null, [location.pathname]);

  const [theme, setTheme] = useState<'light' | 'dark'>(() => {
    const stored = typeof window !== 'undefined' ? localStorage.getItem('theme') : null;
    if (stored === 'dark' || stored === 'light') return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  });

  const [menuOpen, setMenuOpen] = useState(false);

  const toggleTheme = useCallback(() => {
    setTheme((current) => {
      const next = current === 'light' ? 'dark' : 'light';
      document.documentElement.setAttribute('data-theme', next);
      localStorage.setItem('theme', next);
      return next;
    });
  }, []);

  const closeMenu = useCallback(() => setMenuOpen(false), []);

  // Heading content rendered into the topbar by the active page (e.g. the
  // Project Detail page pushes its Back button + breadcrumb + project name).
  // Falls back to the static per-route PAGE_META when a page doesn't set one.
  const [topbarHeading, setTopbarHeading] = useState<ReactNode | null>(null);
  const setTopbarHeadingStable = useCallback((node: ReactNode) => setTopbarHeading(node), []);
  // Reset the custom heading on route change so a stale heading from the
  // previous page doesn't bleed into the next page's first paint.
  useEffect(() => { setTopbarHeading(null); }, [location.pathname]);

  const outletContext: ShellOutletContext = { setTopbarHeading: setTopbarHeadingStable };

  return (
    <div className="app-shell" data-theme={theme}>
      <aside className={`sidebar${menuOpen ? ' is-open' : ''}`}>
        <div className="sidebar-brand">
          <Icon className="icon" name="building-2" size={24} />
          <span>Construction</span>
        </div>
        <nav aria-label="Primary">
          <SidebarNav items={primaryNav} onNavigate={closeMenu} />
          {/* <div className="nav-section-title">Management</div>
          <SidebarNav items={secondaryNav} onNavigate={closeMenu} /> */}
        </nav>
      </aside>

      <div className={`sidebar-overlay${menuOpen ? ' is-visible' : ''}`} onClick={closeMenu} aria-hidden="true" />

      <header className="topbar">
        <div className="topbar-left">
          <button
            type="button"
            className="btn btn-icon btn-ghost menu-toggle"
            onClick={() => setMenuOpen((open) => !open)}
            aria-label="Toggle navigation menu"
            aria-expanded={menuOpen}
          >
            <NavIcon name="menu" />
          </button>
          {topbarHeading ?? (pageMeta && (
            <div className="topbar-page-title">
              <h1 className="topbar-page-title-heading">{pageMeta.title}</h1>
              {pageMeta.subtitle && <p className="topbar-page-title-subtitle">{pageMeta.subtitle}</p>}
            </div>
          ))}
        </div>
        <div className="topbar-right">
          <ThemeToggle theme={theme} onToggle={toggleTheme} />
          <button type="button" className="btn btn-icon btn-ghost" aria-label="Notifications">
            <NavIcon name="bell" />
          </button>
        </div>
      </header>

      <main className="main-content">
        <Outlet context={outletContext} />
      </main>
    </div>
  );
}
