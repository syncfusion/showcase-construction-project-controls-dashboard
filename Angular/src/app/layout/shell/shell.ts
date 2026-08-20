import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NgTemplateOutlet } from '@angular/common';
import { filter, map, startWith } from 'rxjs';
import { primaryNav } from '../navigation';
import { IconComponent } from '../../shared/components/icon';
import { OutletContextService } from '../outlet-context.service';

type Theme = 'light' | 'dark';

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

function readStoredTheme(): Theme {
  const stored = typeof window !== 'undefined' ? localStorage.getItem('theme') : null;
  if (stored === 'dark' || stored === 'light') return stored;
  return typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light';
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, IconComponent, NgTemplateOutlet],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
})
export class Shell {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly outlet = inject(OutletContextService);

  readonly navItems = primaryNav;
  readonly theme = signal<Theme>(readStoredTheme());
  readonly menuOpen = signal(false);

  // Read the current pathname as a signal so `pageMeta()` (a computed) updates
  // automatically on every NavigationEnd.
  private readonly currentPath = signal<string>(this.router.url);
  readonly pageMeta = computed<PageMeta | null>(() => PAGE_META[this.currentPath()] ?? null);

  constructor() {
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        map((e) => e.urlAfterRedirects || e.url),
        startWith(this.router.url),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((url) => {
        this.currentPath.set(url);
        // Reset the custom heading on every route change so a stale heading
        // from the previous page doesn't bleed into the next page's first paint
        // (mirrors React's `useEffect(() => { setTopbarHeading(null); }, [location.pathname])`).
        this.outlet.clearTopbarHeading();
      });
  }

  toggleTheme(): void {
    const next: Theme = this.theme() === 'light' ? 'dark' : 'light';
    this.theme.set(next);
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('theme', next);
  }

  toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
