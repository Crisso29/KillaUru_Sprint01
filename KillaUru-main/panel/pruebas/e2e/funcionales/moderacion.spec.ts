// T069 — E2E moderación (US5, RN-07, FR-027…FR-030). El simulador aplica la tabla RN-07 del backend y recuerda el
// cambio: el detalle que se vuelve a pedir trae el estado nuevo.
import { test, expect, type Page } from '@playwright/test'
import { abrir, simularRespuesta } from '../ayudas'

const ACTIVA = 'a1b2c3d4-0000-0000-0000-000000000001'
const EXPIRADA = 'a1b2c3d4-0000-0000-0000-000000000003'
const REPARADA = 'a1b2c3d4-0000-0000-0000-000000000004'

/** Abre el detalle de una anomalía en el mapa (con su estado en el filtro, como lo enlaza la lista). */
async function abrirDetalle(page: Page, id: string, estado: string) {
  await abrir(page, `/mapa?estado=${estado}&anomalia=${id}`)
  return page.getByTestId('detalle-anomalia')
}

/** Pulsa la acción y confirma en el diálogo. */
async function moderar(page: Page, accion: string) {
  await page.getByRole('button', { name: accion }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Confirmar' }).click()
}

test('marcar reparada desde detalle Activa', async ({ page }) => {
  // Arrange
  const detalle = await abrirDetalle(page, ACTIVA, 'Activa')
  await expect(detalle.getByText('Activa', { exact: true })).toBeVisible()

  // Act
  await moderar(page, 'Marcar como reparada')

  // Assert — nuevo estado y la acción siguiente que permite RN-07
  await expect(detalle.getByText('Reparada', { exact: true })).toBeVisible()
  await expect(detalle.getByRole('button', { name: 'Reabrir' })).toBeEnabled()
})

test('descartar desde Expirada', async ({ page }) => {
  // Arrange
  const detalle = await abrirDetalle(page, EXPIRADA, 'Expirada')

  // Act
  await moderar(page, 'Descartar')

  // Assert
  await expect(detalle.getByText('Descartada', { exact: true })).toBeVisible()
})

test('mantener descartada desde Por revisar', async ({ page }) => {
  // Arrange — dos descartadas volvieron a reportarse
  await abrir(page, '/moderacion')
  const mantener = page.getByRole('button', { name: 'Mantener descartada' })
  await expect(mantener).toHaveCount(2)

  // Act — mantener no cambia lo que ve el conductor: no pide confirmación
  await mantener.first().click()

  // Assert — sale de la lista sin abrir un diálogo
  await expect(mantener).toHaveCount(1)
  await expect(page.getByRole('dialog')).toHaveCount(0)
})

test('reabrir desde Reparada en detalle', async ({ page }) => {
  // Arrange
  const detalle = await abrirDetalle(page, REPARADA, 'Reparada')

  // Act
  await moderar(page, 'Reabrir')

  // Assert
  await expect(detalle.getByText('Activa', { exact: true })).toBeVisible()
  await expect(detalle.getByRole('button', { name: 'Marcar como reparada' })).toBeEnabled()
})

test('409 transicion_no_permitida → aviso y recarga del detalle', async ({ page }) => {
  // Arrange — otro administrador la cambió mientras tanto: el servidor rechaza la transición
  await abrirDetalle(page, ACTIVA, 'Activa')
  await simularRespuesta(page, { metodo: 'post', ruta: '/api/v1/anomalies/:id/moderacion', estado: 409, cuerpo: { codigo: 'transicion_no_permitida' } })
  const recarga = page.waitForRequest((peticion) => peticion.method() === 'GET' && peticion.url().endsWith(`/api/v1/anomalies/${ACTIVA}`))

  // Act
  await moderar(page, 'Marcar como reparada')

  // Assert — se avisa y se vuelve a pedir el detalle para mostrar el estado real
  await expect(page.getByText('Ya no se puede: la anomalía cambió de estado. Actualizamos el detalle.')).toBeVisible()
  await recarga
  await expect(page.getByRole('dialog')).toHaveCount(0)
})
