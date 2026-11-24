// T087, SC-006 — contra el entorno de pruebas, el mapa con sus anomalías queda listo (marca `mapa-listo` de
// LienzoMapa) en ≤ 3 s p95 en 10 cargas. Mide red, CDN y backend reales, que el build simulado no tiene: corre solo con
// `npm run e2e:pruebas` (PANEL_URL del panel desplegado), con E2E_CORREO y E2E_CONTRASENA de un administrador de
// pruebas y el Turnstile de prueba de Cloudflare (siempre aprueba) configurado en ese entorno.
import { test, expect, type Page } from '@playwright/test'

const CARGAS = 10
const P95_MAXIMO_MS = 3000

/** Percentil 95 por el método del rango más cercano: con 10 cargas es la más lenta de las 10. */
const percentil95 = (valores: number[]) =>
  [...valores].sort((menor, mayor) => menor - mayor)[Math.ceil(valores.length * 0.95) - 1]!

/** Milisegundos desde el inicio de la navegación hasta `performance.mark('mapa-listo')`. */
const msHastaMapaListo = (page: Page) => page.evaluate(() => new Promise<number>((resolver) => {
  const leer = () => {
    const marca = performance.getEntriesByName('mapa-listo')[0]
    if (marca) resolver(marca.startTime)
    else setTimeout(leer, 50)
  }
  leer()
}))

test('10 cargas del mapa contra killauru-pruebas: p95 de «mapa listo» ≤ 3 s', async ({ page }) => {
  // Arrange — una sesión de administrador; las cookies se reutilizan en cada carga
  const [correo, contrasena] = [process.env['E2E_CORREO'], process.env['E2E_CONTRASENA']]
  test.skip(!correo || !contrasena, 'Faltan E2E_CORREO y E2E_CONTRASENA del administrador de pruebas')
  await page.goto('/login')
  await page.getByLabel('Correo electrónico').fill(correo!)
  await page.getByLabel('Contraseña').fill(contrasena!)
  await page.getByRole('button', { name: 'Entrar' }).click()
  await page.waitForURL('**/resumen')

  // Act
  const tiempos: number[] = []
  for (let carga = 0; carga < CARGAS; carga++) {
    await page.goto('/mapa')
    tiempos.push(await msHastaMapaListo(page))
  }

  // Assert — se adjuntan los 10 tiempos al informe para la evidencia
  test.info().annotations.push({ type: 'tiempos-ms', description: tiempos.map(Math.round).join(', ') })
  expect(percentil95(tiempos)).toBeLessThanOrEqual(P95_MAXIMO_MS)
})
