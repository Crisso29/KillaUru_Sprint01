// T080 — E2E auditoría (US7, FR-037…FR-039): filtros por categoría y actor, y la moderación en lenguaje claro
import { test, expect, type Page } from '@playwright/test'
import { abrir } from '../ayudas'

const ADMIN_ID = 'a1a1a1a1-0000-0000-0000-000000000001'

/** Espera la consulta de auditoría cuyo parámetro `nombre` vale `valor`. */
const consultaCon = (page: Page, nombre: string, valor: string) => page.waitForRequest((peticion) =>
  peticion.url().includes('/api/v1/auditoria?') && new URL(peticion.url()).searchParams.get(nombre) === valor)

test.beforeEach(async ({ page }) => {
  await abrir(page, '/auditoria')
  await expect(page.getByText(/retención permanente/i)).toBeVisible()
})

test('filtra por categoría y muestra registros', async ({ page }) => {
  // Arrange
  const consulta = consultaCon(page, 'categoria', 'Moderacion')

  // Act
  await page.getByLabel('Categoría').selectOption('Moderacion')

  // Assert
  await consulta
  await expect(page.getByRole('row').filter({ hasText: 'Moderación' }).first()).toBeVisible()
})

test('filtra por UUID de actor y muestra registros', async ({ page }) => {
  // Arrange
  const consulta = consultaCon(page, 'actorId', ADMIN_ID)

  // Act
  await page.getByLabel('Actor (correo o UUID)').fill(ADMIN_ID)

  // Assert — un UUID se usa tal cual, sin buscar la cuenta
  await consulta
  await expect(page.getByRole('row').filter({ hasText: '@killauru.pe' }).first()).toBeVisible()
})

test('filtra por correo de actor y muestra registros', async ({ page }) => {
  // Arrange — el correo se traduce a su actorId con una búsqueda en el directorio
  const busqueda = page.waitForRequest((peticion) => /\/api\/v1\/usuarios\?.*busqueda=admin%40killauru\.pe/.test(peticion.url()))
  const consulta = consultaCon(page, 'actorId', ADMIN_ID)

  // Act
  await page.getByLabel('Actor (correo o UUID)').fill('admin@killauru.pe')

  // Assert
  await busqueda
  await consulta
})

test('AnomaliaModerada (T068) aparece con texto de la acción de moderación', async ({ page }) => {
  // Arrange y Act — beforeEach abrió la auditoría con sus registros
  const fila = page.getByRole('row').filter({ hasText: 'Anomalía marcada como reparada' })

  // Assert — el código AnomaliaModerada + contexto.accion se lee en lenguaje claro
  await expect(fila).toBeVisible()
  await expect(page.getByText('AnomaliaModerada', { exact: true })).toHaveCount(0)
})

test('RolCambiado (T058) aparece con texto del cambio de rol', async ({ page }) => {
  // Arrange y Act — beforeEach abrió la auditoría con sus registros
  const fila = page.getByRole('row').filter({ hasText: 'Rol cambiado' })

  // Assert — el código RolCambiado se muestra en lenguaje claro
  await expect(fila).toBeVisible()
  await expect(page.getByText('RolCambiado', { exact: true })).toHaveCount(0)
})

test('DatasetExportado (T085) aparece con texto de la exportación', async ({ page }) => {
  // Arrange y Act — beforeEach abrió la auditoría con sus registros
  const fila = page.getByRole('row').filter({ hasText: 'Dataset exportado' })

  // Assert — el código DatasetExportado se muestra en lenguaje claro
  await expect(fila).toBeVisible()
  await expect(page.getByText('DatasetExportado', { exact: true })).toHaveCount(0)
})
