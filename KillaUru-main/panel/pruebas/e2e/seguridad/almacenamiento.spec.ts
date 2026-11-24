// T034 — E2E seguridad (FR-006, R-006): tras entrar, el panel no guarda nada en el navegador. Los tokens viajan solo
// en cookies HttpOnly que pone el backend; en el build simulado no hay cookies reales, así que aquí se comprueba que
// el PANEL no escribe ninguna. Que las del backend sean HttpOnly se prueba en el backend (003) y en el entorno de pruebas.
import { test, expect } from '@playwright/test'
import { iniciarSesion } from '../ayudas'

test.beforeEach(async ({ page }) => {
  await iniciarSesion(page)
})

test('localStorage vacío tras login', async ({ page }) => {
  // Arrange y Act — beforeEach entró por la pantalla de login
  const claves = await page.evaluate(() => Object.keys(localStorage))

  // Assert
  expect(claves).toEqual([])
})

test('sessionStorage vacío tras login', async ({ page }) => {
  // Arrange y Act
  const claves = await page.evaluate(() => Object.keys(sessionStorage))

  // Assert
  expect(claves).toEqual([])
})

test('IndexedDB vacío tras login', async ({ page }) => {
  // Arrange y Act
  const bases = await page.evaluate(async () => (await indexedDB.databases()).map((base) => base.name))

  // Assert
  expect(bases).toEqual([])
})

test('document.cookie sin tokens de sesión', async ({ page }) => {
  // Arrange y Act — lo que un script de la página (o uno inyectado) podría leer
  const cookies = await page.evaluate(() => document.cookie)

  // Assert
  expect(cookies).not.toMatch(/acceso|renovacion|token|sesion|jwt/i)
})
