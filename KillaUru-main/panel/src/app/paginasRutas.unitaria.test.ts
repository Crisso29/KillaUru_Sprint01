import { describe, it, expect, vi } from 'vitest'

// MapLibre necesita WebGL y URL.createObjectURL, que jsdom no tiene; aquí solo importa la forma del export.
vi.mock('@/funcionalidades/mapa/ui/LienzoMapa', () => ({ LienzoMapa: () => null }))

/**
 * `rutas.tsx` carga cada página con `lazy(() => import(...))`. Si el módulo ya exporta un `lazy`, React recibe un lazy
 * dentro de otro y la página se cae (error #306): así estuvo el Mapa del panel. Cada export usado por las rutas debe ser
 * un componente (función), no un objeto `lazy`. Patrón AAA.
 */
const PAGINAS: Array<[string, string, () => Promise<Record<string, unknown>>]> = [
  ['login', 'PaginaLogin', () => import('@/funcionalidades/login')],
  ['login', 'PaginaRecuperar', () => import('@/funcionalidades/login')],
  ['cuenta', 'PaginaVerificar', () => import('@/funcionalidades/cuenta')],
  ['cuenta', 'PaginaFijarContrasena', () => import('@/funcionalidades/cuenta')],
  ['mapa', 'PaginaMapa', () => import('@/funcionalidades/mapa/pagina')],
  ['moderacion', 'PaginaModeracion', () => import('@/funcionalidades/moderacion')],
  ['usuarios', 'PaginaUsuarios', () => import('@/funcionalidades/usuarios')],
  ['auditoria', 'PaginaAuditoria', () => import('@/funcionalidades/auditoria')],
  ['exportacion', 'PaginaExportacion', () => import('@/funcionalidades/exportacion')],
  ['app', 'ResumenConectado', () => import('./ResumenConectado')],
]

describe('Páginas que carga rutas.tsx', () => {
  it.each(PAGINAS)('%s exporta %s como componente, no como lazy', async (_m, nombre, cargar) => {
    // Arrange y Act
    const modulo = await cargar()
    // Assert
    expect(typeof modulo[nombre]).toBe('function')
  })
})
