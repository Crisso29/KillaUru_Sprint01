// T057 — E2E usuarios (US6, FR-031…FR-033): lista, cambio de rol, invitación y revisión mensual de privilegios
import { test, expect, type Page } from '@playwright/test'
import { abrir } from '../ayudas'

/** Espera el POST/PUT a esa ruta y devuelve su cuerpo JSON. */
const cuerpoDe = (page: Page, metodo: string, ruta: RegExp) =>
  page.waitForRequest((peticion) => peticion.method() === metodo && ruta.test(peticion.url())).then((peticion) => peticion.postDataJSON())

test.beforeEach(async ({ page }) => {
  await abrir(page, '/usuarios')
})

test('admin puede ver la lista de usuarios', async ({ page }) => {
  // Arrange y Act — beforeEach abrió Usuarios
  const filas = page.getByRole('row').filter({ hasText: '@killauru.pe' })

  // Assert — las 4 cuentas del directorio simulado, con su correo
  await expect(filas).toHaveCount(4)
  await expect(page.getByRole('row', { name: /conductor@killauru\.pe/ })).toBeVisible()
})

test('admin puede cambiar el rol de un conductor', async ({ page }) => {
  // Arrange
  await page.getByRole('row', { name: /conductor@killauru\.pe/ }).getByRole('button', { name: 'Acciones' }).click()
  await page.getByRole('menuitem', { name: 'Cambiar rol' }).click()
  const dialogo = page.getByRole('dialog', { name: 'Cambiar rol' })
  await dialogo.getByLabel('Nuevo rol').selectOption('Administrador')
  const cuerpo = cuerpoDe(page, 'PUT', /\/api\/v1\/usuarios\/[^/]+\/rol$/)

  // Act
  await dialogo.getByRole('button', { name: 'Confirmar' }).click()

  // Assert
  expect(await cuerpo).toMatchObject({ rol: 'Administrador' })
  await expect(dialogo).toHaveCount(0)
})

test('admin puede invitar a un nuevo usuario', async ({ page }) => {
  // Arrange
  await page.getByRole('button', { name: 'Invitar usuario' }).click()
  const dialogo = page.getByRole('dialog', { name: 'Invitar usuario' })
  await dialogo.getByLabel('Correo electrónico').fill('nueva.admin@killauru.pe')
  const cuerpo = cuerpoDe(page, 'POST', /\/api\/v1\/usuarios$/)

  // Act
  await dialogo.getByRole('button', { name: 'Invitar usuario' }).click()

  // Assert
  expect(await cuerpo).toMatchObject({ correo: 'nueva.admin@killauru.pe' })
  await expect(dialogo).toHaveCount(0)
})

test('admin puede desactivar una cuenta con motivo', async ({ page }) => {
  // Arrange
  await page.getByRole('row', { name: /conductor@killauru\.pe/ }).getByRole('button', { name: 'Acciones' }).click()
  await page.getByRole('menuitem', { name: 'Desactivar cuenta' }).click()
  const dialogo = page.getByRole('dialog', { name: 'Desactivar cuenta' })
  await dialogo.getByRole('textbox', { name: /motivo/i }).fill('No cumple requisitos vigentes')
  const cuerpo = cuerpoDe(page, 'POST', /\/api\/v1\/usuarios\/[^/]+\/desactivar$/)

  // Act
  await dialogo.getByRole('button', { name: 'Confirmar' }).click()

  // Assert
  expect(await cuerpo).toMatchObject({ motivo: 'No cumple requisitos vigentes' })
  await expect(dialogo).toHaveCount(0)
})

test('admin puede activar una cuenta desactivada', async ({ page }) => {
  // Arrange
  const peticion = page.waitForRequest((p) => p.method() === 'POST' && /\/api\/v1\/usuarios\/[^/]+\/activar$/.test(p.url()))

  // Act
  await page.getByRole('row', { name: /desactivado@killauru\.pe/ }).getByRole('button', { name: 'Acciones' }).click()
  await page.getByRole('menuitem', { name: 'Activar cuenta' }).click()

  // Assert
  await peticion
})

test('admin puede reenviar la invitación pendiente', async ({ page }) => {
  // Arrange
  const peticion = page.waitForRequest((p) => p.method() === 'POST' && /\/api\/v1\/usuarios\/[^/]+\/reenviar-correo$/.test(p.url()))

  // Act
  await page.getByRole('row', { name: /pendiente@killauru\.pe/ }).getByRole('button', { name: 'Acciones' }).click()
  await page.getByRole('menuitem', { name: 'Reenviar invitación' }).click()

  // Assert
  await peticion
})

test('admin puede registrar revisión de privilegios', async ({ page }) => {
  // Arrange — una decisión por administrador (FR-033)
  await page.getByRole('button', { name: 'Hacer revisión' }).click()
  const dialogo = page.getByRole('dialog', { name: 'Revisión de privilegios' })
  for (const confirmar of await dialogo.getByRole('radio', { name: 'Confirmar' }).all()) await confirmar.check()
  const cuerpo = cuerpoDe(page, 'POST', /\/api\/v1\/usuarios\/revision-privilegios$/)

  // Act
  await dialogo.getByRole('button', { name: 'Guardar' }).click()

  // Assert — todos confirmados, ninguno retirado
  const enviado = await cuerpo
  expect(enviado.retirados).toEqual([])
  expect(enviado.confirmados.length).toBeGreaterThan(0)
  await expect(dialogo).toHaveCount(0)
})
