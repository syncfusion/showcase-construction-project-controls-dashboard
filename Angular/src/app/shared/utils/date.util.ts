const formatter = new Intl.DateTimeFormat(undefined, { year: 'numeric', month: 'short', day: 'numeric' });

export function formatDate(value: string | Date | number | null | undefined): string {
  if (value === null || value === undefined) return '—';
  const date = typeof value === 'string' || typeof value === 'number' ? new Date(value) : value;
  if (Number.isNaN(date.getTime())) return '—';
  return formatter.format(date);
}
