import { defineConfig, devices } from '@playwright/test'

// T012: e2e contra build simulado
export default defineConfig({
  testDir: './pruebas/e2e',
  testMatch: '**/*.spec.ts',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: 'http://localhost:4173',
    // serviceWorkers: 'block' — MSW activa el SW en Chromium/Edge y responde peticiones directamente
    // sin pasar por la red; page.route() nunca las intercepta, por lo que simularRespuesta() no tiene
    // efecto. Bloqueando el SW, page.route() intercepta todas las peticiones en todos los navegadores.
    serviceWorkers: 'block',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'msedge', use: { ...devices['Desktop Edge'] } },
  ],
  webServer: {
    command: 'npm run build:simulado && npm run preview',
    url: 'http://localhost:4173',
    reuseExistingServer: !process.env['CI'],
    timeout: 180_000,
  },
})
