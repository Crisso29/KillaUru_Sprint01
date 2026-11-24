// T050 — E2E mapa (US2, FR-021…FR-026, SC-001): primera consulta, filtros en la dirección y lista navegable
import { test, expect } from '@playwright/test'
import { abrir } from '../ayudas'

const EXPIRADA = 'a1b2c3d4-0000-0000-0000-000000000003'

test('primera petición sin filtros lleva estado=Activa,PendienteConfirmacion (SC-001)', async ({ page }) => {
  // Arrange
  const consulta = page.waitForRequest((peticion) => /\/api\/v1\/anomalies\?/.test(peticion.url()))

  // Act
  await abrir(page, '/mapa')

  // Assert — lo que pide atención primero; el resto se elige con los filtros
  expect(new URL((await consulta).url()).searchParams.get('estado')).toBe('Activa,PendienteConfirmacion')
})

test('cambiar filtros actualiza la dirección', async ({ page }) => {
  // Arrange
  await abrir(page, '/mapa')
  await page.getByRole('button', { name: /^Estado:/ }).click()

  // Act
  await page.getByRole('checkbox', { name: 'Pendiente de confirmar' }).uncheck()

  // Assert — la dirección guarda el filtro y la consulta lo usa
  await expect(page).toHaveURL(/[?&]estado=Activa(&|$)/)
  await expect(page.getByRole('button', { name: 'Estado: Activa' })).toBeVisible()
})

test('la dirección copiada abre la misma vista', async ({ page }) => {
  // Arrange — un enlace con filtro y anomalía elegida, como el que se copia de la barra
  const enlace = `/mapa?estado=Expirada&anomalia=${EXPIRADA}`

  // Act
  await abrir(page, enlace)

  // Assert — mismos filtros y el detalle de esa anomalía abierto
  await expect(page.getByRole('button', { name: 'Estado: Expirada' })).toBeVisible()
  const detalle = page.getByTestId('detalle-anomalia')
  await expect(detalle.getByRole('heading', { name: /Bache · leve/i })).toBeVisible()
  await expect(detalle.getByText('Expirada', { exact: true })).toBeVisible()
})

test('la lista es navegable con teclado', async ({ page }) => {
  // Arrange — el foco en la primera fila de la lista
  await abrir(page, '/mapa')
  await page.getByRole('button', { name: /Bache · km 287,4/ }).focus()
  await page.keyboard.press('Tab')
  await expect(page.getByRole('button', { name: /Hundimiento · km 310,0/ })).toBeFocused()

  // Act
  await page.keyboard.press('Enter')

  // Assert — Enter abre el detalle de la fila enfocada y la dirección lo guarda
  await expect(page).toHaveURL(/anomalia=a1b2c3d4-0000-0000-0000-000000000002/)
  await expect(page.getByTestId('detalle-anomalia').getByRole('heading', { name: /Hundimiento/ })).toBeVisible()
})
