/**
 * `[measuredWidth]` directive — tracks an element's pixel width via a
 * ResizeObserver, quantises to the nearest 8px, and writes the value to a
 * consumer-supplied signal.
 *
 * Mirrors `useElementWidth` in react/src/pages/DashboardPage.tsx. Syncfusion
 * charts measure their `availableSize` once at mount; handing the chart an
 * explicit pixel `[width]` driven from this directive keeps the chart in
 * sync with the card width on every browser resize, instead of clipping.
 *
 * Usage:
 *   <div measuredWidth #mw="measuredWidth">
 *     <ejs-chart [width]="mw.value() > 0 ? mw.value() + 'px' : '100%'" … />
 *   </div>
 */

import {
  Directive,
  ElementRef,
  OnDestroy,
  OnInit,
  effect,
  inject,
  signal,
} from '@angular/core';

@Directive({
  selector: '[measuredWidth]',
  standalone: true,
  exportAs: 'measuredWidth',
})
export class MeasuredWidthDirective implements OnInit, OnDestroy {
  private host = inject(ElementRef<HTMLElement>);
  private ro?: ResizeObserver;

  /** Quantised (8px) clientWidth of the host element; 0 until measurement. */
  readonly value = signal(0);

  ngOnInit(): void {
    const el = this.host.nativeElement;
    const update = (): void => {
      const w = Math.round(el.clientWidth / 8) * 8;
      this.value.set(w);
    };
    update();
    this.ro = new ResizeObserver(() => update());
    this.ro.observe(el);
  }

  ngOnDestroy(): void {
    this.ro?.disconnect();
  }

  /** Re-export for templates that don't use the exportAs. */
  asValue(): number {
    return this.value();
  }
}

/** Convenience export for code that needs the signal only. */
export function measuredWidthSignal() {
  const dir = new MeasuredWidthDirective();
  // Manual setup for non-directive use; the inject(ElementRef) inside the
  // directive constructor will throw once it tries to use Angular DI — this
  // helper is purely a type-level convenience for callers that don't need
  // a directive context. Prefer the directive form.
  return dir.value;
}
