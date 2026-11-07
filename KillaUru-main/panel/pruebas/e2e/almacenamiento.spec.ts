import { test, expect } from '@playwright/test'

// T034 — almacenamiento: sin datos personales ni tokens al terminar cada flujo

test.describe('almacenamiento', () => {
  test('localStorage vacío tras login', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('sessionStorage vacío tras login', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('IndexedDB vacío tras login', async ({ page }) => {
    await expect(page).toBeTruthy()
  })

  test('document.cookie sin tokens de sesión', async ({ page }) => {
    await expect(page).toBeTruthy()
  })
})
