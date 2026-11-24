// T032 — E2E sesión (FR-007, R-006, data-model §1): renovación ante 401, sesión rechazada y salida entre pestañas
import { test, expect } from '@playwright/test'
import { abrir, simularRespuesta } from '../ayudas'

const AUDITORIA = '/api/v1/auditoria'
const TOKEN_VENCIDO = { codigo: 'token_invalido' }

test('401 renovado → la pantalla sigue sin ir al login', async ({ page }) => {
  // Arrange — el token de acceso vence justo al pedir la auditoría; la renovación responde bien
  await abrir(page, '/resumen')
  await simularRespuesta(page, { metodo: 'get', ruta: AUDITORIA, estado: 401, cuerpo: TOKEN_VENCIDO, veces: 1 })
  const renovacion = page.waitForRequest((peticion) => peticion.url().endsWith('/api/v1/auth/renovar'))

  // Act
  await page.getByRole('link', { name: 'Auditoría', exact: true }).click()

  // Assert — renovó, reintentó y mostró los datos sin pasar por el login
  await renovacion
  await expect(page.getByText(/retención permanente/i)).toBeVisible()
  await expect(page).toHaveURL(/\/auditoria$/)
})

test('renovación rechazada → login con aviso de sesión terminada', async ({ page }) => {
  // Arrange — con la sesión ya activa (el panel renovó al abrir), el servidor la revoca: el token vence y la
  // renovación también responde 401. Si el escenario se pusiera antes, alcanzaría la renovación inicial.
  await abrir(page, '/resumen')
  await expect(page.getByRole('heading', { name: 'Resumen' })).toBeVisible()
  await simularRespuesta(page, { metodo: 'post', ruta: '/api/v1/auth/renovar', estado: 401, cuerpo: { codigo: 'sesion_invalida' } })
  await simularRespuesta(page, { metodo: 'get', ruta: AUDITORIA, estado: 401, cuerpo: TOKEN_VENCIDO })

  // Act
  await page.getByRole('link', { name: 'Auditoría', exact: true }).click()

  // Assert
  await expect(page).toHaveURL(/\/login\?motivo=sesion$/)
  await expect(page.getByRole('status')).toHaveText('Tu sesión terminó. Vuelve a entrar.')
})

test('Salir en una pestaña lleva a la otra al login', async ({ context }) => {
  // Arrange — dos pestañas del panel con la misma sesión
  const primera = await context.newPage()
  const segunda = await context.newPage()
  await abrir(primera, '/resumen')
  await abrir(segunda, '/mapa')

  // Act
  await primera.getByRole('button', { name: 'Salir' }).click()

  // Assert — la otra pestaña también sale (BroadcastChannel), sin el aviso de sesión perdida
  await expect(primera).toHaveURL(/\/login$/)
  await expect(segunda).toHaveURL(/\/login$/)
})
