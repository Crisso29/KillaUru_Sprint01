// T085 — E2E exportación (US9, FR-034…FR-036): descarga CSV o GeoJSON con lo elegido en el formulario
import { test, expect } from '@playwright/test'
import { readFileSync } from 'fs'
import { abrir } from '../ayudas'

const esExportacion = (peticion: { method: () => string; url: () => string }) =>
  peticion.method() === 'POST' && peticion.url().endsWith('/api/v1/exportaciones')

test.beforeEach(async ({ page }) => {
  await abrir(page, '/exportacion')
})

test('admin puede exportar en formato CSV', async ({ page }) => {
  // Arrange — CSV es el formato por defecto
  const pedido = page.waitForRequest(esExportacion)
  const descarga = page.waitForEvent('download')

  // Act
  await page.getByRole('button', { name: 'Exportar' }).click()

  // Assert — sin rango elegido, el archivo se llama solo con el prefijo
  expect((await pedido).postDataJSON()).toMatchObject({ formato: 'CSV', tipo: null })
  expect((await descarga).suggestedFilename()).toBe('killauru-anomalias.csv')
})

test('admin puede exportar en formato GeoJSON con filtro de tipo', async ({ page }) => {
  // Arrange
  await page.getByLabel('Tipo').selectOption('Bache')
  await page.getByRole('radio', { name: /GeoJSON/ }).check()
  const pedido = page.waitForRequest(esExportacion)
  const descarga = page.waitForEvent('download')

  // Act
  await page.getByRole('button', { name: 'Exportar' }).click()

  // Assert
  expect((await pedido).postDataJSON()).toMatchObject({ formato: 'GeoJSON', tipo: 'Bache' })
  expect((await descarga).suggestedFilename()).toBe('killauru-anomalias.geojson')
})

test('la cabecera del CSV es exactamente la del contrato 002', async ({ page }) => {
  // Arrange
  const descarga = page.waitForEvent('download')

  // Act
  await page.getByRole('button', { name: 'Exportar' }).click()
  const archivo = await descarga
  const ruta = await archivo.path()

  // Assert
  const cabecera = readFileSync(ruta!, 'utf8').split('\n')[0]!.trim()
  expect(cabecera).toBe('id,tipo,severidad,estado,persistencia,km,latitud,longitud,reportes,primer_reporte,ultimo_reporte')
})

test('la sección Recientes muestra las exportaciones anteriores', async ({ page }) => {
  // Arrange — espera que la página y las consultas asíncronas terminen
  await page.waitForLoadState('networkidle')

  // Act — la sección es parte de la página ya cargada
  const encabezado = page.getByRole('heading', { name: /recientes/i })

  // Assert — la sección existe y la tabla tiene al menos una fila de exportación previa
  await expect(encabezado).toBeVisible()
  await expect(page.locator('section').filter({ has: encabezado }).locator('tbody tr').first()).toBeVisible()
})
