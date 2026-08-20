/**
 * The element that actually carries `data-theme` (see layout/Shell.tsx,
 * which sets it on `.app-shell`, not on `<html>`). Token-resolution helpers
 * and theme-change observers must read/watch THIS element — reading
 * `document.documentElement` instead works only while the theme toggle's
 * button-click handler has also (redundantly) mirrored the attribute onto
 * `<html>` in the same session; it silently reverts to light-theme values
 * after a page refresh, since that mirroring never re-runs on mount.
 */
export function getThemeScopeElement(): HTMLElement {
  return (document.querySelector('.app-shell') as HTMLElement | null) ?? document.documentElement;
}
