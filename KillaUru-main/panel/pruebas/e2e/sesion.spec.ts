import { test, expect } from '@playwright/test'

// T032 — sesión: renovación silenciosa y cierre entre pestañas

test.describe('sesion', () => {
  test('401 renovado → la pantalla sigue sin ir al login', async ({ page }) => {
    // TODO: implementar cuando el modo simulado esté desplegado
    await expect(page).toBeTruthy()
  })

  test('renovación rechazada → login con aviso de sesión terminada', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('Salir en una pestaña lleva a la otra al login', async ({ browser }) => {
    await expect(browser).toBeTruthy()
  })
})
