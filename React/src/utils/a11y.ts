import type { KeyboardEvent } from 'react';

/**
 * Keyboard equivalent for elements using role="button" (e.g. clickable table rows
 * and cards) so Enter/Space activate them the same way a click would.
 */
export function onActivateKey(handler: () => void) {
  return (e: KeyboardEvent) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      handler();
    }
  };
}
