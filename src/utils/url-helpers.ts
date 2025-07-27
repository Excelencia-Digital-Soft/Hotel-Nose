/**
 * URL utility functions for consistent API endpoint handling
 */

/**
 * Ensures URL ends with a forward slash
 * @param {string} url - The URL to normalize
 * @returns {string} - URL with trailing slash
 */
export function ensureTrailingSlash(url: string): string {
  if (!url) return '/';
  return url.endsWith('/') ? url : `${url}/`;
}

/**
 * Removes trailing slash from URL if present
 * @param {string} url - The URL to normalize
 * @returns {string} - URL without trailing slash
 */
export function removeTrailingSlash(url: string): string {
  if (!url) return '';
  return url.endsWith('/') ? url.slice(0, -1) : url;
}

/**
 * Build WebSocket URL with proper formatting
 * @param {string} baseUrl - Base API URL
 * @param {string} endpoint - WebSocket endpoint (default: 'notifications')
 * @returns {string} - Properly formatted WebSocket URL
 */
export function buildWebSocketUrl(baseUrl: string, endpoint: string = 'notifications'): string {
  const normalizedBase = ensureTrailingSlash(baseUrl);
  return `${normalizedBase}${endpoint}`;
}

/**
 * Build API URL with proper formatting
 * @param {string} baseUrl - Base API URL
 * @param {string} endpoint - API endpoint
 * @returns {string} - Properly formatted API URL
 */
export function buildApiUrl(baseUrl: string, endpoint: string): string {
  const normalizedBase = ensureTrailingSlash(baseUrl);
  // Remove leading slash from endpoint if present
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${normalizedBase}${cleanEndpoint}`;
}