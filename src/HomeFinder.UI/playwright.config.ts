import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  timeout: 30_000,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: 'http://127.0.0.1:5187',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: 'dotnet run --project ../tests/e2e-host/HomeFinder.E2eHost.csproj --configuration Release --no-launch-profile',
      url: 'http://127.0.0.1:5017/__e2e/ready',
      env: { DOTNET_ENVIRONMENT: 'E2E', ASPNETCORE_ENVIRONMENT: 'E2E' },
      timeout: 180_000,
      reuseExistingServer: false,
    },
    {
      command: 'pnpm run build:e2e && pnpm exec vite preview --config vite.e2e.config.ts --host 127.0.0.1 --port 5187 --strictPort',
      url: 'http://127.0.0.1:5187',
      env: { VITE_API_BASE_URL: 'http://127.0.0.1:5017' },
      timeout: 120_000,
      reuseExistingServer: false,
    },
  ],
});
