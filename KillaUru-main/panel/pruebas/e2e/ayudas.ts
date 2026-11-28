import { expect, type Page, type Route } from '@playwright/test'
import { registrarRutas } from './simEstado'

export async function abrir(page: Page, ruta: string) {
  await registrarRutas(page)
  await page.goto(ruta)
  await page.waitForFunction(() => Boolean(window.__killauruSimulado))
}

export interface Escenario {
  metodo: 'get' | 'post' | 'put' | 'delete'
  /** Ruta de la API sin la base, p. ej. `/api/v1/auth/inicio-sesion`; admite `:id`. */
  ruta: string
  estado: number
  cuerpo?: unknown
  /** Cuántas veces responde así antes de volver a la respuesta normal; sin valor, siempre. */
  veces?: number
}

/**
 * Cambia la respuesta simulada de una ruta hasta recargar la página. El último escenario agregado tiene prioridad
 * (Playwright evalúa rutas en orden LIFO); agotadas sus `veces`, la petición pasa al anterior.
 */
export async function simularRespuesta(page: Page, escenario: Escenario) {
  const { ruta, estado, cuerpo, veces } = escenario
  const pattern = `**${ruta.replace(/:([^/]+)/g, '*')}`
  const handler = async (route: Route) => {
    if (cuerpo !== undefined) await route.fulfill({ status: estado, json: cuerpo as Record<string, unknown> })
    else await route.fulfill({ status: estado })
  }
  if (veces !== undefined) await page.route(pattern, handler, { times: veces })
  else await page.route(pattern, handler)
}

/** Login completo por la pantalla: el Turnstile simulado entrega el token y se espera el Resumen. */
export async function iniciarSesion(page: Page, correo = 'ops@killauru.pe', contrasena = 'Demo1234!') {
  await completarLogin(page, correo, contrasena)
  await page.getByRole('button', { name: 'Entrar' }).click()
  await page.waitForURL('**/resumen')
}

/** Escribe las credenciales y espera a que el desafío anti-bot habilite «Entrar» (sin pulsarlo). */
export async function completarLogin(page: Page, correo = 'ops@killauru.pe', contrasena = 'Demo1234!') {
  if (!page.url().includes('/login')) await abrir(page, '/login')
  await page.getByLabel('Correo').fill(correo)
  await page.getByLabel('Contraseña').fill(contrasena)
  await expect(page.getByRole('button', { name: 'Entrar' })).toBeEnabled()
}
