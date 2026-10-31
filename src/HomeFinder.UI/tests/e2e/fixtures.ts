import { test as base, expect } from '@playwright/test';

export const apiUrl = 'http://127.0.0.1:5017';
export const fullAccessHeaders = { Authorization: 'Bearer e2e-user' };
type Persona = 'user' | 'reader' | 'no-roles' | 'anonymous';

export const test = base.extend<{ persona: Persona; resetData: void; assertNoEntra: void }>({
  persona: ['user', { option: true }],
  storageState: async ({ persona }, use) => {
    await use({
      cookies: [],
      origins: [{
        origin: 'http://127.0.0.1:5187',
        localStorage: persona === 'anonymous' ? [] : [{ name: 'homefinder.e2e.token', value: `e2e-${persona}` }],
      }],
    });
  },
  resetData: [async ({ request }, use) => {
    const response = await request.post(`${apiUrl}/__e2e/reset`, { headers: fullAccessHeaders });
    expect(response.status(), await response.text()).toBe(204);
    await use();
  }, { auto: true }],
  assertNoEntra: [async ({ page }, use) => {
    const entraRequests: string[] = [];
    page.context().on('request', request => {
      const host = new URL(request.url()).hostname;
      if (host.endsWith('microsoftonline.com') || host.endsWith('b2clogin.com') || host.endsWith('ciamlogin.com')) {
        entraRequests.push(request.url());
      }
    });
    await use();
    expect(entraRequests, 'E2E 実行中に Entra への通信が発生しました。').toEqual([]);
  }, { auto: true }],
});

export { expect };
