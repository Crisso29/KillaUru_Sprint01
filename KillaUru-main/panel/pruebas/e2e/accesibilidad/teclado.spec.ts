// T088 — E2E teclado (FR-043, WCAG 2.1.1 y 2.4.3): los flujos principales se completan sin ratón y, al abrir un
// diálogo, el foco entra en él.
import { test, expect, type Locator, type Page } from '@playwright/test'
import { abrir } from '../ayudas'

/** Pulsa Tab hasta que `destino` tenga el foco (como haría una persona con teclado); falla si no llega. */
async function tabularHasta(page: Page, destino: Locator, maximo = 40) {
  for (let pulsaciones = 0; pulsaciones < maximo; pulsaciones++) {
    if (await destino.evaluate((elemento) => elemento === document.activeElement)) return
    await page.keyboard.press('Tab')
  }
  await expect(destino).toBeFocused()
}

/** El foco está dentro del diálogo abierto. */
const focoDentro = (dialogo: Locator) => dialogo.evaluate((elemento) => elemento.contains(document.activeElement))

test('login con Tab y Enter', async ({ page }) => {
  // Arrange
  await abrir(page, '/login')
  await tabularHasta(page, page.getByLabel('Correo electrónico'))
  await page.keyboard.type('ops@killauru.pe')
  await page.keyboard.press('Tab')
  await page.keyboard.type('Demo1234!')
  await expect(page.getByRole('button', { name: 'Entrar' })).toBeEnabled()

  // Act — Enter en la contraseña envía el formulario
  await page.keyboard.press('Enter')

  // Assert
  await expect(page).toHaveURL(/\/resumen$/)
})

test('filtros del mapa con teclado', async ({ page }) => {
  // Arrange — abre el menú «Estado» con Enter y llega a «Pendiente de confirmar» con Tab
  await abrir(page, '/mapa')
  const boton = page.getByRole('button', { name: /^Estado:/ })
  await tabularHasta(page, boton)
  await page.keyboard.press('Enter')
  await tabularHasta(page, page.getByRole('checkbox', { name: 'Pendiente de confirmar' }))

  // Act — Espacio desmarca y Escape cierra
  await page.keyboard.press('Space')
  await page.keyboard.press('Escape')

  // Assert — filtro aplicado y el foco vuelve al botón (sin trampa de teclado, WCAG 2.1.2)
  await expect(page).toHaveURL(/[?&]estado=Activa(&|$)/)
  await expect(page.getByRole('group', { name: 'Estado' })).toHaveCount(0)
  await expect(boton).toBeFocused()
})

test('lista → detalle → moderar con teclado', async ({ page }) => {
  // Arrange — de la lista al detalle con Enter, y a «Marcar como reparada» con Tab
  await abrir(page, '/mapa')
  await tabularHasta(page, page.getByRole('button', { name: /Bache · km 287,4/ }))
  await page.keyboard.press('Enter')
  await tabularHasta(page, page.getByRole('button', { name: 'Marcar como reparada' }))
  await page.keyboard.press('Enter')
  const dialogo = page.getByRole('dialog')
  expect(await focoDentro(dialogo), 'al abrirse, el foco debe entrar en el diálogo (WCAG 2.4.3)').toBe(true)
  await tabularHasta(page, dialogo.getByRole('button', { name: 'Confirmar' }))

  // Act
  await page.keyboard.press('Enter')

  // Assert
  await expect(page.getByTestId('detalle-anomalia').getByText('Reparada', { exact: true })).toBeVisible()
})

test('revisión mensual de privilegios con teclado', async ({ page }) => {
  // Arrange — abre la revisión, marca «Confirmar» en cada administrador con Espacio y llega a «Guardar»
  await abrir(page, '/usuarios')
  await tabularHasta(page, page.getByRole('button', { name: 'Hacer revisión' }))
  await page.keyboard.press('Enter')
  const dialogo = page.getByRole('dialog', { name: 'Revisión de privilegios' })
  expect(await focoDentro(dialogo), 'al abrirse, el foco debe entrar en el diálogo (WCAG 2.4.3)').toBe(true)
  for (const confirmar of await dialogo.getByRole('radio', { name: 'Confirmar' }).all()) {
    await tabularHasta(page, confirmar)
    await page.keyboard.press('Space')
  }
  await tabularHasta(page, dialogo.getByRole('button', { name: 'Guardar' }))
  const guardado = page.waitForRequest((peticion) => peticion.url().endsWith('/api/v1/usuarios/revision-privilegios'))

  // Act
  await page.keyboard.press('Enter')

  // Assert
  expect((await guardado).postDataJSON().retirados).toEqual([])
  await expect(dialogo).toHaveCount(0)
})
