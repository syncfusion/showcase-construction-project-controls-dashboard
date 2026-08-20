/**
 * Theme-bridge utilities for Syncfusion components.
 *
 * Mirrors `frontend/Construction.React/src/utils/theme.ts` so Angular and
 * React resolve CSS-token values the same way:
 *
 *  - `themeScopeElement()` returns `.app-shell` (where Angular's theme toggle
 *    writes `data-theme`, see `app/layout/shell/shell.ts`) and falls back to
 *    `<html>`. Syncfusion palettes read literal color / pixel strings — those
 *    strings must be resolved at render time, not memoised beyond a theme
 *    revision, so token values track light/dark switches.
 *
 *  - `SyncfusionTokensService` exposes a `revision` signal that bumps on:
 *      • `MutationObserver` on `.app-shell` for `class`/`data-theme` changes
 *        (Shell's `toggleTheme()` writes both attributes)
 *      • `prefers-color-scheme` MQ changes (system dark/light)
 *      • one initial bump inside `init()` to compensate for the
 *        before-mount vs after-commit race the React version documents
 *
 *  Pages inject this service and reference `tokens.revision()` inside any
 * `computed()` that derives chart palette so the chart re-resolves via
 * Signal effects whenever the user toggles the theme.
 */

import { Injectable, signal } from '@angular/core';

// The DOM `document` and `window` globals are part of the project's `lib` so they
// have non-optional types. A naive `typeof document === 'undefined'` check would
// narrow `document` to `never` and trip the TS2339 against `.documentElement`.
// We test against `globalThis` instead — that property exists on every host
// and `typeof` is the right operator for runtime feature detection.
const hasDocument = (): boolean => typeof (globalThis as { document?: unknown }).document !== 'undefined';
const hasWindow = (): boolean => typeof (globalThis as { window?: unknown }).window !== 'undefined';

export function themeScopeElement(): HTMLElement {
  if (!hasDocument()) return (globalThis as { document: { documentElement: HTMLElement } }).document.documentElement;
  const doc = (globalThis as { document: Document }).document;
  return (doc.querySelector('.app-shell') as HTMLElement | null) ?? doc.documentElement;
}

export function resolveTokenColor(name: string, fallback: string): string {
  if (!hasWindow()) return fallback;
  const value = getComputedStyle(themeScopeElement()).getPropertyValue(name).trim();
  return value || fallback;
}

export function resolveTokenPx(name: string, fallbackPx: number): number {
  if (!hasWindow()) return fallbackPx;
  const raw = getComputedStyle(themeScopeElement()).getPropertyValue(name).trim();
  if (!raw) return fallbackPx;
  if (raw.endsWith('px')) return Number.parseFloat(raw.slice(0, -2)) || fallbackPx;
  const num = Number.parseFloat(raw);
  return Number.isFinite(num) ? num : fallbackPx;
}

@Injectable({ providedIn: 'root' })
export class SyncfusionTokensService {
  readonly revision = signal(0);

  private observer?: MutationObserver;
  private mq?: MediaQueryList;
  private initialised = false;

  /** Wire up theme-change listeners. Idempotent. Call from each page's ngOnInit. */
  init(): void {
    if (this.initialised || typeof (globalThis as { window?: unknown }).window === 'undefined') return;
    this.initialised = true;

    // First bump after commit — mirrors React's pre-mount bump().
    queueMicrotask(() => this.bump());

    const target = themeScopeElement();
    this.observer = new MutationObserver(() => this.bump());
    this.observer.observe(target, {
      attributes: true,
      attributeFilter: ['class', 'data-theme'],
    });

    this.mq = (globalThis as { window: Window }).window.matchMedia('(prefers-color-scheme: dark)');
    this.mq.addEventListener('change', this.bump);
  }

  bump = (): void => {
    this.revision.update((n) => n + 1);
  };

  /** Optional teardown for unit tests / route leave hooks. */
  destroy(): void {
    this.observer?.disconnect();
    this.observer = undefined;
    this.mq?.removeEventListener('change', this.bump);
    this.initialised = false;
  }
}
