// T012: ayudas reutilizables para pruebas e2e Playwright
import { type Page } from '@playwright/test'

export async function iniciarSesion(page: Page, correo = 'ops@killauru.pe', contrasena = 'Demo1234!') {
  await page.goto('/login')
  await page.fill('[name="correo"]', correo)
  await page.fill('[name="contrasena"]', contrasena)
  await page.click('[type="submit"]')
  await page.waitForURL('**/resumen')
}

export async function cerrarSesion(page: Page) {
  await page.click('[data-testid="boton-salir"]')
  await page.waitForURL('**/login')
}

export async function irA(page: Page, ruta: string) {
  await page.goto(ruta)
  await page.waitForLoadState('networkidle')
}
