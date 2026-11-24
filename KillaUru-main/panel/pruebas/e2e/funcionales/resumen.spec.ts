// T077 — E2E Resumen: carga, cambio de periodo y navegación a Moderación (FR-015…FR-018)
import { test, expect } from '@playwright/test'
import { abrir } from '../ayudas'

test.describe('Resumen', () => {
  test.beforeEach(async ({ page }) => {
    await abrir(page, '/resumen')
  })

  test('muestra los 6 rótulos de indicadores', async ({ page }) => {
    // Arrange y Act — beforeEach abre el Resumen
    const indicadores = page.getByRole('region', { name: 'Indicadores de negocio' })

    // Assert
    await expect(indicadores).toBeVisible()
    await expect(page.getByText('Eventos válidos (ayer)')).toBeVisible()
    await expect(page.getByText('Conductores activos')).toBeVisible()
    await expect(page.getByText('Dispositivos 24 h')).toBeVisible()
    await expect(page.getByText('Cobertura')).toBeVisible()
    await expect(page.getByRole('heading', { name: 'Confirmación', exact: true })).toBeVisible()
    await expect(page.getByText('Reaparición')).toBeVisible()
  })

  test('selector de periodo envía dias=7 al cambiar a 7 d', async ({ page }) => {
    // Arrange
    // MSW responde desde el service worker; page.route no intercepta esas respuestas.
    const peticion = page.waitForRequest(peticion =>
      peticion.url().includes('/api/v1/dashboard/indicadores?dias=7'))

    // Act — se pulsa la opción visible «7 d», como el usuario (el radio está oculto para lectores de pantalla)
    await page.locator('label.seg-opcion', { hasText: '7 d' }).click()

    // Assert
    expect(new URL((await peticion).url()).searchParams.get('dias')).toBe('7')
    await expect(page.getByText('Eventos válidos (ayer)')).toBeVisible()
  })

  test('«Paneles técnicos» abre en nueva pestaña', async ({ page, context }) => {
    // Arrange y Act
    const [nuevaPagina] = await Promise.all([
      context.waitForEvent('page'),
      page.getByRole('link', { name: /paneles técnicos/i }).click(),
    ])

    // Assert
    expect(nuevaPagina).toBeTruthy()
  })

  test('«Requiere atención» permite navegar a Moderación', async ({ page }) => {
    // Arrange y Act
    await page.getByRole('link', { name: /puntos descartados/i }).click()

    // Assert
    await expect(page).toHaveURL(/\/moderacion/)
  })
})
