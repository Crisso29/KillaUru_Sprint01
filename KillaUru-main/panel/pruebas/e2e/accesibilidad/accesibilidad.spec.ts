// T086 — Accesibilidad (FR-043): 0 violaciones axe wcag2a + wcag2aa en cada pantalla del panel y con diálogos abiertos.
// Las rutas son las de `src/app/rutas.tsx` (antes la lista tenía rutas que no existían, como `/cuenta/verificar` sin página).
import { test, expect } from '@playwright/test'
import AxeBuilder from '@axe-core/playwright'
import { abrir } from '../ayudas'

const PANTALLAS: Array<[string, string]> = [
  ['/login', 'Iniciar sesión'],
  ['/recuperar', 'Recuperar contraseña'],
  ['/cuenta/verificar#token=t', 'Verificar correo'],
  ['/cuenta/restablecer#token=t', 'Nueva contraseña'],
  ['/cuenta/invitacion#token=t', 'Nueva contraseña'],
  ['/resumen', 'Resumen'],
  ['/mapa', 'Mapa'],
  ['/moderacion', 'Moderación'],
  ['/usuarios', 'Usuarios'],
  ['/auditoria', 'Auditoría'],
  ['/exportacion', 'Exportaciones'],
]

for (const [ruta, nombre] of PANTALLAS) {
  test(`${ruta} — 0 violaciones`, async ({ page }) => {
    // Arrange — la pantalla cargada con sus datos (no el esqueleto de carga)
    await abrir(page, ruta)
    await expect(page.getByText(nombre).first()).toBeVisible()
    await page.waitForLoadState('networkidle')

    // Act
    const { violations } = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).analyze()

    // Assert — el detalle dice qué regla falla y en qué elementos
    expect(violations.map(({ id, nodes }) => `${id}: ${nodes.map((nodo) => nodo.target.join(' ')).join(' | ')}`)).toEqual([])
  })
}

test('/mapa — diálogo de moderación — 0 violaciones', async ({ page }) => {
  // Arrange — abre el detalle de una anomalía activa y dispara el diálogo de descarte
  await abrir(page, '/mapa?estado=Activa&anomalia=a1b2c3d4-0000-0000-0000-000000000001')
  const detalle = page.getByTestId('detalle-anomalia')
  await expect(detalle.getByText('Activa', { exact: true })).toBeVisible()
  await detalle.getByRole('button', { name: 'Descartar' }).click()
  await expect(page.getByRole('dialog')).toBeVisible()
  await page.waitForLoadState('networkidle')

  // Act — solo el diálogo: la violación scrollable-region pertenece al canvas de MapLibre, no al diálogo
  const { violations } = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).include('[role="dialog"]').analyze()

  // Assert
  expect(violations.map(({ id, nodes }) => `${id}: ${nodes.map((nodo) => nodo.target.join(' ')).join(' | ')}`)).toEqual([])
})

test('/usuarios — diálogo de invitación — 0 violaciones', async ({ page }) => {
  // Arrange — abre la pantalla y el diálogo de invitar usuario
  await abrir(page, '/usuarios')
  await expect(page.getByRole('heading', { name: 'Usuarios' })).toBeVisible()
  await page.getByRole('button', { name: 'Invitar usuario' }).click()
  await expect(page.getByRole('dialog', { name: 'Invitar usuario' })).toBeVisible()
  await page.waitForLoadState('networkidle')

  // Act — solo el diálogo
  const { violations } = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa']).include('[role="dialog"]').analyze()

  // Assert
  expect(violations.map(({ id, nodes }) => `${id}: ${nodes.map((nodo) => nodo.target.join(' ')).join(' | ')}`)).toEqual([])
})
