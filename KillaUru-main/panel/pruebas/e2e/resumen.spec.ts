// T077 — E2E Resumen: carga, cambio de periodo y navegación a Moderación (FR-015…FR-018)
import { test, expect } from '@playwright/test'

test.describe('Resumen', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/resumen')
  })

  test('muestra los 6 rótulos de indicadores', async ({ page }) => {
    await expect(page.getByText('Eventos válidos (ayer)')).toBeVisible()
    await expect(page.getByText('Conductores activos')).toBeVisible()
    await expect(page.getByText('Dispositivos 24 h')).toBeVisible()
    await expect(page.getByText('Cobertura')).toBeVisible()
    await expect(page.getByText('Confirmación')).toBeVisible()
    await expect(page.getByText('Reaparición')).toBeVisible()
  })

  test('selector de periodo envía dias=7 al cambiar a 7 d', async ({ page }) => {
    let diasEnviado = ''
    await page.route('**/api/v1/dashboard/indicadores*', (route) => {
      const url = new URL(route.request().url())
      diasEnviado = url.searchParams.get('dias') ?? ''
      void route.continue()
    })
    await page.getByLabel('7 días').click()
    await expect(page.getByText('Eventos válidos (ayer)')).toBeVisible()
    expect(diasEnviado).toBe('7')
  })

  test('«Paneles técnicos» abre en nueva pestaña', async ({ page, context }) => {
    const [nuevaPagina] = await Promise.all([
      context.waitForEvent('page'),
      page.getByRole('link', { name: /paneles técnicos/i }).click(),
    ])
    expect(nuevaPagina).toBeTruthy()
  })

  test('«Requiere atención» permite navegar a Moderación', async ({ page }) => {
    await page.getByRole('link', { name: /puntos descartados/i }).click()
    await expect(page).toHaveURL(/\/moderacion/)
  })
})
