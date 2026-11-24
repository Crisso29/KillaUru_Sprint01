// T033 — E2E login (US1, FR-005, R-020) contra el build simulado: el Turnstile simulado entrega el token y cada
// escenario de error se elige con `simularRespuesta`.
import { test, expect } from '@playwright/test'
import { abrir, completarLogin, simularRespuesta } from '../ayudas'

const LOGIN = '/api/v1/auth/inicio-sesion'

test.beforeEach(async ({ page }) => {
  await abrir(page, '/login')
})

test('entrar con credenciales correctas lleva al resumen', async ({ page }) => {
  // Arrange
  await completarLogin(page)

  // Act
  await page.getByRole('button', { name: 'Entrar' }).click()

  // Assert
  await expect(page).toHaveURL(/\/resumen$/)
  await expect(page.getByRole('heading', { name: 'Resumen' })).toBeVisible()
})

test('credenciales incorrectas muestran error', async ({ page }) => {
  // Arrange
  await simularRespuesta(page, { metodo: 'post', ruta: LOGIN, estado: 401, cuerpo: { codigo: 'credenciales_invalidas' } })
  await completarLogin(page, 'ops@killauru.pe', 'ClaveEquivocada1')

  // Act
  await page.getByRole('button', { name: 'Entrar' }).click()

  // Assert
  await expect(page.getByRole('alert')).toHaveText('Correo o contraseña incorrectos.')
  await expect(page).toHaveURL(/\/login$/)
})

test('bloqueo 423 tras 3 fallos muestra aviso', async ({ page }) => {
  // Arrange — dos intentos fallidos y el tercero bloquea la cuenta 15 min (R-020)
  const hasta = new Date(Date.now() + 15 * 60_000).toISOString()
  await simularRespuesta(page, { metodo: 'post', ruta: LOGIN, estado: 423, cuerpo: { codigo: 'cuenta_bloqueada', bloqueadoHastaUtc: hasta } })
  await simularRespuesta(page, { metodo: 'post', ruta: LOGIN, estado: 401, cuerpo: { codigo: 'credenciales_invalidas' }, veces: 2 })
  for (let intento = 1; intento <= 2; intento++) {
    await completarLogin(page, 'ops@killauru.pe', 'ClaveEquivocada1')
    await page.getByRole('button', { name: 'Entrar' }).click()
    await expect(page.getByRole('alert')).toHaveText('Correo o contraseña incorrectos.')
  }
  await completarLogin(page, 'ops@killauru.pe', 'ClaveEquivocada1')

  // Act
  await page.getByRole('button', { name: 'Entrar' }).click()

  // Assert — el aviso reemplaza al formulario y dice cuándo se puede volver a intentar
  await expect(page.getByRole('alert')).toContainText('Vuelve a intentar a las')
  await expect(page.getByRole('button', { name: 'Entrar' })).toHaveCount(0)
})

test('conductor recibe «Esta cuenta no tiene acceso al panel.»', async ({ page }) => {
  // Arrange
  await simularRespuesta(page, { metodo: 'post', ruta: LOGIN, estado: 403, cuerpo: { codigo: 'solo_administradores' } })
  await completarLogin(page, 'conductor@killauru.pe', 'ClaveDeConductor1')

  // Act
  await page.getByRole('button', { name: 'Entrar' }).click()

  // Assert
  await expect(page.getByRole('alert')).toHaveText('Esta cuenta no tiene acceso al panel.')
})
