// T062 — E2E cuenta (US8, FR-013, FR-014): los enlaces de los correos, tal como los arma el backend (/cuenta/…#token=)
import { test, expect } from '@playwright/test'
import { abrir, simularRespuesta } from '../ayudas'

test('link válido verifica el correo y muestra confirmación', async ({ page }) => {
  // Arrange
  const verificacion = page.waitForRequest((peticion) => peticion.url().endsWith('/api/v1/auth/verificar-correo'))

  // Act
  await abrir(page, '/cuenta/verificar#token=token-de-prueba')

  // Assert — envía el token y lo quita de la barra (no queda en el historial)
  expect((await verificacion).postDataJSON()).toEqual({ token: 'token-de-prueba' })
  await expect(page.getByRole('status')).toHaveText('Listo, tu correo está verificado.')
  expect(new URL(page.url()).hash).toBe('')
})

test('link vencido muestra error de enlace inválido', async ({ page }) => {
  // Arrange — el simulador listo antes de abrir el enlace, con el token ya vencido
  await abrir(page, '/login')
  await simularRespuesta(page, { metodo: 'post', ruta: '/api/v1/auth/verificar-correo', estado: 400, cuerpo: { codigo: 'enlace_invalido' } })

  // Act — navegación dentro de la aplicación: el escenario simulado se conserva (recargar lo borraría)
  await page.evaluate(() => { history.pushState({}, '', '/cuenta/verificar#token=vencido'); dispatchEvent(new PopStateEvent('popstate')) })

  // Assert
  await expect(page.getByRole('alert')).toHaveText('El enlace no es válido o ya venció. Pide uno nuevo desde la app.')
})

test('flujo activar cuenta por invitación completo', async ({ page }) => {
  // Arrange — el enlace llega como /cuenta/invitacion#token=
  await abrir(page, '/cuenta/invitacion#token=token-de-prueba')
  await page.getByLabel('Nueva contraseña').fill('ClaveNueva2026!')
  await page.getByLabel('Confirmar contraseña').fill('ClaveNueva2026!')

  // Act
  await page.getByRole('button', { name: 'Guardar contraseña' }).click()

  // Assert — confirma con el mensaje de invitación y el hash ya no está en la barra
  await expect(page.getByRole('status')).toHaveText('Tu cuenta está lista. Ya puedes iniciar sesión.')
  expect(new URL(page.url()).hash).toBe('')
})

test('flujo restablecer contraseña completo', async ({ page }) => {
  // Arrange — pide el enlace y lo abre como llega en el correo
  await abrir(page, '/recuperar')
  await page.getByLabel('Correo electrónico').fill('ops@killauru.pe')
  await page.getByRole('button', { name: 'Enviar enlace' }).click()
  await expect(page.getByRole('status')).toBeVisible()
  await abrir(page, '/cuenta/restablecer#token=token-de-prueba')
  await page.getByLabel('Nueva contraseña').fill('ClaveNueva2026!')
  await page.getByLabel('Confirmar contraseña').fill('ClaveNueva2026!')

  // Act
  await page.getByRole('button', { name: 'Guardar contraseña' }).click()

  // Assert — confirma y lleva al inicio de sesión
  await expect(page.getByRole('status')).toHaveText('Tu contraseña cambió. Cerramos todas tus sesiones: vuelve a entrar.')
  await page.getByRole('link', { name: 'Ir al inicio de sesión' }).click()
  await expect(page).toHaveURL(/\/login$/)
})
