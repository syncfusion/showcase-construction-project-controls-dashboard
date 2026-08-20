import { Injectable, TemplateRef, signal } from '@angular/core';

/**
 * Mirrors React's `ShellOutletContext.setTopbarHeading(node)` API. Project
 * Detail (and any other page that wants to render custom content in the
 * topbar's left slot — Back + breadcrumb + project name, for example)
 * calls `setTopbarHeading(tpl)`. The Shell reads `topbarHeading()` and
 * falls back to the static `pageMeta` when no template is set.
 *
 * Cleared by `clearTopbarHeading()` on route change (the Shell subscribes
 * to Router events in the constructor to keep parity with the React app,
 * which does the same in a `useEffect` keyed on `location.pathname`).
 */
@Injectable({ providedIn: 'root' })
export class OutletContextService {
  private readonly _topbarHeading = signal<TemplateRef<unknown> | null>(null);

  /** Current topbar heading template, or null when the page hasn't set one. */
  readonly topbarHeading = this._topbarHeading.asReadonly();

  /** Page writes its custom topbar content here. */
  setTopbarHeading(tpl: TemplateRef<unknown> | null): void {
    this._topbarHeading.set(tpl);
  }

  /** Called by the Shell on route change so a stale heading from the
   *  previous page doesn't bleed into the next page's first paint. */
  clearTopbarHeading(): void {
    this._topbarHeading.set(null);
  }
}
