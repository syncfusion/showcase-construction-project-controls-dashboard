import type { ReactElement } from 'react';

export function NotFoundPage(): ReactElement {
  return (
    <div className="card" style={{ maxWidth: 480, margin: 'var(--space-3xl) auto' }}>
      <h1 className="card-title">Page not found</h1>
      <p className="text-secondary">The requested page does not exist.</p>
    </div>
  );
}
