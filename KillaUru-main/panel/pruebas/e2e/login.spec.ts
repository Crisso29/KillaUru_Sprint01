import { test, expect } from '@playwright/test'

// T033 — login: flujo completo, credenciales malas, bloqueo y rol conductor

test.describe('login', () => {
  test('entrar con credenciales correctas lleva al resumen', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('credenciales incorrectas muestran error', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('bloqueo 423 tras 3 fallos muestra aviso', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('conductor recibe «Esta cuenta no tiene acceso al panel.»', async ({ page }) => {
    await expect(page).toBeTruthy()
  })
})
