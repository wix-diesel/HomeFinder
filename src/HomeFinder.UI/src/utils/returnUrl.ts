/**
 * returnUrl が同一オリジンの相対パスかどうかを検証する（オープンリダイレクト防止）
 */
export function isSafeReturnUrl(url: unknown): url is string {
  if (typeof url !== 'string' || !url) {
    return false;
  }

  try {
    const parsed = new URL(url, window.location.origin);
    return parsed.origin === window.location.origin && url.startsWith('/');
  } catch {
    return false;
  }
}

/**
 * returnUrl を検証し、安全な同一オリジン相対パスのみを返す
 */
export function sanitizeReturnUrl(url: unknown): string {
  if (typeof url !== 'string' || !url) {
    return '/';
  }

  const sanitizedUrl = url.split('#')[0] || '/';
  return isSafeReturnUrl(sanitizedUrl) ? sanitizedUrl : '/';
}
