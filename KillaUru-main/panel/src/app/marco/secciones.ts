import type { NombreIcono } from '@/compartido/ui'

/** Una entrada del menú lateral: ruta, texto e ícono del prototipo (`NAV` de killauru-diseno.html). */
export interface Seccion {
  ruta: string
  texto: string
  icono: NombreIcono
}

/** Menú del panel en dos grupos, en el orden del prototipo. Las migas de la barra superior salen de aquí. */
export const GRUPOS: readonly { titulo: string; secciones: readonly Seccion[] }[] = [
  {
    titulo: 'Operación',
    secciones: [
      { ruta: '/resumen', texto: 'Resumen', icono: 'home' },
      { ruta: '/mapa', texto: 'Mapa', icono: 'map' },
      { ruta: '/moderacion', texto: 'Moderación', icono: 'flag' },
    ],
  },
  {
    titulo: 'Administración',
    secciones: [
      { ruta: '/usuarios', texto: 'Usuarios', icono: 'users' },
      { ruta: '/auditoria', texto: 'Auditoría', icono: 'log' },
      { ruta: '/exportacion', texto: 'Exportaciones', icono: 'file' },
    ],
  },
]

/** «Operación / Resumen» para la ruta actual; la raíz `/` es el Resumen. */
export function migas(ruta: string): { grupo: string; seccion: string } | null {
  const buscada = ruta === '/' ? '/resumen' : ruta
  for (const g of GRUPOS) {
    const s = g.secciones.find((x) => buscada.startsWith(x.ruta))
    if (s) return { grupo: g.titulo, seccion: s.texto }
  }
  return null
}
