// T012: ayudas reutilizables para pruebas e2e Playwright contra el build simulado (MSW en el navegador)
import { expect, type Page } from '@playwright/test'

/** Lo que el modo simulado deja en `window` (pruebas/msw/navegador.ts → exponerParaPruebas). */
interface Simulado {
  worker: { use: (...manejadores: unknown[]) => void }
  http: Record<'get' | 'post' | 'put' | 'delete', (ruta: string, resolver: () => Response | undefined) => unknown>
  HttpResponse: { json: (cuerpo: unknown, init?: ResponseInit) => Response }
  base: string
}

declare global {
  interface Window { __killauruSimulado?: Simulado }
}

/** Abre una ruta y espera a que el simulador (MSW) esté listo para responder y para recibir escenarios. */
export async function abrir(page: Page, ruta: string) {
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
 * Cambia la respuesta simulada de una ruta hasta recargar la página. El último escenario agregado tiene prioridad;
 * agotadas sus `veces`, la petición pasa al anterior (así se arman secuencias: 2 × 401 y luego 423).
 */
export async function simularRespuesta(page: Page, escenario: Escenario) {
  await page.evaluate(({ metodo, ruta, estado, cuerpo, veces }) => {
    const { worker, http, HttpResponse, base } = window.__killauruSimulado!
    let restantes = veces ?? Number.POSITIVE_INFINITY
    worker.use(http[metodo](`${base}${ruta}`, () => {
      if (restantes-- <= 0) return undefined // MSW pasa la petición al siguiente manejador
      return cuerpo === undefined ? new Response(null, { status: estado }) : HttpResponse.json(cuerpo, { status: estado })
    }))
  }, escenario)
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
