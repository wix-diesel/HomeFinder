export function normalizeJan(value: string): string {
  return value.replace(/[^0-9]/g, '').trim();
}

export function isValidJan(value: string): boolean {
  const jan = normalizeJan(value);
  return /^\d{8}$/.test(jan) || /^\d{13}$/.test(jan);
}
