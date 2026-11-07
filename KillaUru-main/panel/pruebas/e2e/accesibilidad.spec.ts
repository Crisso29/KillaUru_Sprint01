// T086 — Accesibilidad: 0 violaciones axe wcag2a + wcag2aa en todas las rutas (stubs)
import { test } from '@playwright/test'

// Placeholder — implementar en T090 con @axe-core/playwright
test.describe('Accesibilidad (axe wcag2a + wcag2aa)', () => {
  const rutas = [
    '/',
    '/mapa',
    '/resumen',
    '/moderacion',
    '/usuarios',
    '/auditoria',
    '/exportacion',
    '/cuenta/verificar',
    '/cuenta/restablecer',
    '/cuenta/invitacion',
    '/login',
  ]

  for (const ruta of rutas) {
    test.skip(`${ruta} — 0 violaciones`, async () => {
      // TODO T090: const { violations } = await checkA11y(page)
    })
  }
})
