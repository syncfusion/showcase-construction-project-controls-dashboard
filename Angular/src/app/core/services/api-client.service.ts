import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { API_BASE_URL } from '../../config';

// Build a full request URL from the configured base + a relative path,
// normalising the join so we don't end up with `//api/...` or `/api/`.
function joinUrl(base: string, path: string): string {
  const trimmedBase = base.replace(/\/+$/, '');
  const trimmedPath = path.replace(/^\/+/, '');
  return `${trimmedBase}/${trimmedPath}`;
}

function buildParams(params?: Record<string, string | number | boolean | undefined>): HttpParams {
  let httpParams = new HttpParams();
  if (!params) return httpParams;
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      httpParams = httpParams.set(key, String(value));
    }
  }
  return httpParams;
}

function toError(err: HttpErrorResponse): Observable<never> {
  // Shape returned by the backend's DevelopmentApiExceptionMiddleware.
  // { status: number, error: string, message: string, hint?: string, stackTrace?: string }
  const structured = isStructuredErrorBody(err.error);
  if (err.status === 0) {
    // Network-level failure (DNS, connection refused, CORS preflight rejection,
    // offline, dev-server not proxying). Browser message is the most useful clue.
    const underlying = err.message || 'Network error — is the API running?';
    return throwError(() => new Error(underlying));
  }
  if (structured) {
    // 503 / 500 with a structured body — use the message + hint, ignore the
    // generic HTTP prefix so the page-level alert reads naturally.
    const composed = structured.hint
      ? `${structured.message}\n\n${structured.hint}`
      : structured.message;
    return throwError(() => new Error(composed));
  }
  // Fallback: plain-text body or a non-JSON error payload.
  const message = typeof err.error === 'string' && err.error ? err.error : `HTTP ${err.status} — ${err.statusText || 'request failed'}`;
  return throwError(() => new Error(message));
}

interface StructuredErrorBody {
  status: number;
  error: string;
  message: string;
  hint?: string;
  stackTrace?: string;
}

function isStructuredErrorBody(value: unknown): StructuredErrorBody | null {
  if (!value || typeof value !== 'object') return null;
  const v = value as Partial<StructuredErrorBody>;
  if (typeof v.message === 'string' && typeof v.error === 'string') {
    return v as StructuredErrorBody;
  }
  return null;
}

@Injectable({ providedIn: 'root' })
export class ApiClientService {
  private http = inject(HttpClient);

  getJson<T>(path: string, params?: Record<string, string | number | boolean | undefined>): Observable<T> {
    return this.http
      .get<T>(joinUrl(API_BASE_URL, path), { params: buildParams(params) })
      .pipe(catchError(toError));
  }

  postJson<T>(path: string, body?: unknown): Observable<T> {
    return this.http.post<T>(joinUrl(API_BASE_URL, path), body ?? {}).pipe(catchError(toError));
  }

  putJson<T>(path: string, body?: unknown): Observable<T> {
    return this.http.put<T>(joinUrl(API_BASE_URL, path), body ?? {}).pipe(catchError(toError));
  }

  deleteJson<T>(path: string): Observable<T> {
    return this.http.delete<T>(joinUrl(API_BASE_URL, path)).pipe(catchError(toError));
  }
}
