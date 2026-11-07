import { defineConfig, devices } from '@playwright/test'

// T012: e2e contra build simulado
export default defineConfig({
  testDir: './pruebas/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:4173',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'msedge', use: { ...devices['Desktop Edge'] } },
    {
      name: 'chromium-pruebas',
      use: { ...devices['Desktop Chrome'] },
      testMatch: '**/*.pruebas.ts',
    },
  ],
  webServer: {
    command: 'npm run build:simulado && npm run preview',
    url: 'http://localhost:4173',
    reuseExistingServer: !process.env['CI'],
  },
})
