import { defineConfig, devices } from '@playwright/test'

// Entorno desplegado: sin servidor local ni MSW. Los casos reales siguen pendientes.
const baseURL = process.env['PANEL_URL']
if (!baseURL || new URL(baseURL).protocol !== 'https:') {
  throw new Error('PANEL_URL debe ser la URL HTTPS del panel en el entorno de pruebas.')
}

export default defineConfig({
  testDir: './pruebas/e2e',
  testMatch: '**/*.pruebas.ts',
  forbidOnly: true,
  workers: 1,
  retries: 0,
  reporter: [['list'], ['html', { outputFolder: 'playwright-report/desplegado', open: 'never' }]],
  use: { baseURL, trace: 'retain-on-failure' },
  projects: [{ name: 'chromium-pruebas', use: { ...devices['Desktop Chrome'] } }],
})
