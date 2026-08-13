import type { ReactElement } from 'react';
import { Icon } from './Icon';
import './Pagination.css';

export interface PaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export function Pagination({ page, pageSize, totalCount, onPageChange }: PaginationProps): ReactElement | null {
  if (totalCount <= pageSize) return null;

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <div className="pagination" role="navigation" aria-label="Pagination">
      <button
        type="button"
        className="btn btn-ghost btn-sm"
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
        aria-label="Previous page"
      >
        <Icon className="icon" name="chevron-left" size={16} />
      </button>
      <span className="pagination-info">
        Page <strong>{page}</strong> of {totalPages}
      </span>
      <button
        type="button"
        className="btn btn-ghost btn-sm"
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
        aria-label="Next page"
      >
        <Icon className="icon" name="chevron-right" size={16} />
      </button>
    </div>
  );
}
