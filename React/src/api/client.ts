import { API_BASE_URL } from '../config';

function joinUrl(base: string, path: string): string {
  if (path.startsWith('http')) return path;
  const baseWithSlash = base.endsWith('/') ? base : `${base}/`;
  const cleanPath = path.startsWith('/') ? path.slice(1) : path;
  return `${baseWithSlash}${cleanPath}`;
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const message = await response.text().catch(() => `HTTP ${response.status}`);
    throw new Error(message || `HTTP ${response.status}`);
  }
  return response.json() as Promise<T>;
}

export async function getJson<T>(path: string, params?: Record<string, string | number | boolean | undefined>): Promise<T> {
  const url = new URL(joinUrl(API_BASE_URL, path), window.location.origin);
  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        url.searchParams.set(key, String(value));
      }
    });
  }

  const response = await fetch(url.toString(), {
    headers: { Accept: 'application/json' },
  });
  return handleResponse<T>(response);
}

export async function postJson<T>(path: string, body: unknown): Promise<T> {
  const response = await fetch(joinUrl(API_BASE_URL, path), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify(body),
  });
  return handleResponse<T>(response);
}

export async function putJson<T>(path: string, body: unknown): Promise<T> {
  const response = await fetch(joinUrl(API_BASE_URL, path), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify(body),
  });
  return handleResponse<T>(response);
}

export async function deleteJson<T>(path: string): Promise<T> {
  const response = await fetch(joinUrl(API_BASE_URL, path), {
    method: 'DELETE',
    headers: { Accept: 'application/json' },
  });
  return handleResponse<T>(response);
}
