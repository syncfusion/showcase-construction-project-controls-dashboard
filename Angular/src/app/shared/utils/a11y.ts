/**
 * Keyboard equivalent for elements using role="button" (e.g. clickable table rows
 * and cards) so Enter/Space activate them the same way a click would.
 */
export function onActivateKey(event: KeyboardEvent, handler: () => void): void {
  if (event.key === 'Enter' || event.key === ' ') {
    event.preventDefault();
    handler();
  }
}
