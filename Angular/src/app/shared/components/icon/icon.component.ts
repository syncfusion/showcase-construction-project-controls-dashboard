/**
 * Lucide icon wrapper for `@lucide/angular` v1.x.
 *
 * Mirrors the React `Icon` component API: a stable string `name`
 * (kebab-case) + numeric `size`. Internally maps the name to the
 * matching `Lucide…` standalone component from `@lucide/angular` and
 * renders the per-icon SVG with the right attribute selector
 * (`<svg lucidePascalCase [size]="N" [strokeWidth]="2">`).
 *
 * Pattern is lifted directly from
 * `showcase-healthcare-appointment-ops/Angular/src/app/layout/app-layout.component.html`
 * where each `Lucide…` component is used inline as `<svg lucidePascalCase [size]="N">`.
 *
 * Why a wrapper:
 *   - The React `Icon` accepts a string name. Keeping the same API in
 *     Angular means every existing template stays identical and a new
 *     icon is a one-line addition in the LUCIDE_ICONS map.
 *   - Several Lucide icons were renamed in v0.469 (Oct 2024). The React
 *     `lucide-react` package preserves the old names as aliases, but
 *     the new `@lucide/angular` exports only canonical names. The map
 *     below translates the React (old) name to the Angular (new) class
 *     so templates can keep using the same name string as the React app
 *     while the implementation picks the correct class.
 *
 * Usage:
 *   <app-icon name="search" [size]="16" />
 *   <app-icon name="chevron-right" [size]="14" />
 *
 * Unknown names render nothing (mirrors React's `if (!Component) return null`).
 */
import { ChangeDetectionStrategy, Component, Type, input } from '@angular/core';
import {
  LucideLayoutDashboard,
  LucideBriefcase,
  LucideWallet,
  // alert-triangle was renamed to triangle-alert in Lucide v0.469.
  LucideTriangleAlert,
  LucideBarChart3,
  LucideMenu,
  LucideBell,
  LucideBuilding2,
  LucideSun,
  LucideMoon,
  LucideX,
  LucideChevronLeft,
  LucideChevronRight,
  // receipt was renamed to receipt-text in Lucide v0.469.
  LucideReceiptText,
  LucideArrowUpRight,
  LucideArrowDownRight,
  LucideFileWarning,
  LucidePlus,
  LucideSearch,
  LucideDownload,
  LucideArrowLeft,
  LucideClock,
  // check-circle was renamed to circle-check in Lucide v0.469.
  LucideCircleCheck,
  LucideFilePlus,
  LucideShieldAlert,
  LucideExternalLink,
  LucideMessageSquare,
  LucideFileText,
  LucideActivity,
  // alert-circle was renamed to circle-alert in Lucide v0.469.
  LucideCircleAlert,
} from '@lucide/angular';
import { LucideIconName } from './icon-names';

/**
 * Icon name surface (kebab-case) the app uses. Mirrors the React app's
 * `IconName` type so the same name strings work in both codebases.
 */
const LUCIDE_ICONS: Record<string, Type<unknown>> = {
  'layout-dashboard': LucideLayoutDashboard,
  briefcase: LucideBriefcase,
  wallet: LucideWallet,
  'alert-triangle': LucideTriangleAlert,
  'bar-chart-3': LucideBarChart3,
  menu: LucideMenu,
  bell: LucideBell,
  'building-2': LucideBuilding2,
  sun: LucideSun,
  moon: LucideMoon,
  x: LucideX,
  'chevron-left': LucideChevronLeft,
  'chevron-right': LucideChevronRight,
  receipt: LucideReceiptText,
  'arrow-up-right': LucideArrowUpRight,
  'arrow-down-right': LucideArrowDownRight,
  'file-warning': LucideFileWarning,
  plus: LucidePlus,
  search: LucideSearch,
  download: LucideDownload,
  'arrow-left': LucideArrowLeft,
  clock: LucideClock,
  'check-circle': LucideCircleCheck,
  'file-plus': LucideFilePlus,
  'shield-alert': LucideShieldAlert,
  'external-link': LucideExternalLink,
  'message-square': LucideMessageSquare,
  'file-text': LucideFileText,
  activity: LucideActivity,
  'alert-circle': LucideCircleAlert,
};

// Re-export the type for consumers (type-only — keeps the icon map
// out of consumer bundles).
export type { LucideIconName };

@Component({
  selector: 'app-icon',
  standalone: true,
  // All Lucide per-icon classes are added to `imports` so their
  // `svg[lucideXxx]` selectors are registered and the @switch template
  // can render any of them. Angular's tree-shaker will only bundle the
  // ones actually referenced by the template, so the runtime bundle
  // size remains proportional to actual icon usage.
  imports: [
    LucideLayoutDashboard,
    LucideBriefcase,
    LucideWallet,
    LucideTriangleAlert,
    LucideBarChart3,
    LucideMenu,
    LucideBell,
    LucideBuilding2,
    LucideSun,
    LucideMoon,
    LucideX,
    LucideChevronLeft,
    LucideChevronRight,
    LucideReceiptText,
    LucideArrowUpRight,
    LucideArrowDownRight,
    LucideFileWarning,
    LucidePlus,
    LucideSearch,
    LucideDownload,
    LucideArrowLeft,
    LucideClock,
    LucideCircleCheck,
    LucideFilePlus,
    LucideShieldAlert,
    LucideExternalLink,
    LucideMessageSquare,
    LucideFileText,
    LucideActivity,
    LucideCircleAlert,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @switch (name()) {
      @case ('layout-dashboard') { <svg lucideLayoutDashboard [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('briefcase') { <svg lucideBriefcase [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('wallet') { <svg lucideWallet [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('alert-triangle') { <svg lucideTriangleAlert [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('bar-chart-3') { <svg lucideBarChart3 [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('menu') { <svg lucideMenu [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('bell') { <svg lucideBell [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('building-2') { <svg lucideBuilding2 [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('sun') { <svg lucideSun [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('moon') { <svg lucideMoon [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('x') { <svg lucideX [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('chevron-left') { <svg lucideChevronLeft [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('chevron-right') { <svg lucideChevronRight [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('receipt') { <svg lucideReceiptText [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('arrow-up-right') { <svg lucideArrowUpRight [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('arrow-down-right') { <svg lucideArrowDownRight [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('file-warning') { <svg lucideFileWarning [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('plus') { <svg lucidePlus [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('search') { <svg lucideSearch [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('download') { <svg lucideDownload [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('arrow-left') { <svg lucideArrowLeft [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('clock') { <svg lucideClock [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('check-circle') { <svg lucideCircleCheck [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('file-plus') { <svg lucideFilePlus [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('shield-alert') { <svg lucideShieldAlert [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('external-link') { <svg lucideExternalLink [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('message-square') { <svg lucideMessageSquare [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('file-text') { <svg lucideFileText [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('activity') { <svg lucideActivity [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
      @case ('alert-circle') { <svg lucideCircleAlert [size]="size()" [strokeWidth]="strokeWidth()" [color]="color()" [attr.aria-hidden]="ariaHidden() ? 'true' : null"></svg> }
    }
  `,
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        line-height: 1;
        vertical-align: middle;
      }
      :host > svg {
        display: inline-flex;
        width: var(--app-icon-size, 16px);
        height: var(--app-icon-size, 16px);
      }
    `,
  ],
  host: {
    '[style.--app-icon-size.px]': 'size()',
  },
})
export class IconComponent {
  /**
   * Accepts any string so callers can store icon names in plain
   * `string`-typed fields (e.g. `NavigationItem.icon`, KPI `kpi.icon`,
   * the `kpiIconFor(key)` lookup on Risks). Unknown strings render
   * nothing (mirrors React's `if (!Component) return null`).
   *
   * If the string is a known icon name, the @switch template renders
   * the matching Lucide SVG.
   */
  readonly name = input.required<string>();
  readonly size = input<number>(16);
  readonly strokeWidth = input<number>(2);
  readonly color = input<string>('currentColor');
  readonly ariaHidden = input<boolean>(true);
}



