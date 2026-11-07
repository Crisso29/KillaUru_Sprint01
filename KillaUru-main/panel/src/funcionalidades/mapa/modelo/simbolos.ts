// T043 — simbolos: 25 combinaciones de forma × severidad para íconos de mapa (FR-023)

export type FormaIcono = 'circulo' | 'rombo' | 'triangulo' | 'cuadrado' | 'hexagono'

export const ESTADOS_MAPA = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada'] as const
export const SEVERIDADES = [1, 2, 3, 4, 5] as const

const FORMA_POR_ESTADO: Record<string, FormaIcono> = {
  Activa: 'circulo',
  PendienteConfirmacion: 'rombo',
  Expirada: 'triangulo',
  Reparada: 'cuadrado',
  Descartada: 'hexagono',
}

export function formaParaEstado(estado: string): FormaIcono {
  return FORMA_POR_ESTADO[estado] ?? 'circulo'
}

export function tieneAnillo(estado: string, severidad: number): boolean {
  return estado === 'Activa' && severidad >= 4
}

export function claveIcono(estado: string, severidad: number): string {
  const forma = formaParaEstado(estado)
  const sufijo = tieneAnillo(estado, severidad) ? '-anillo' : ''
  return `${forma}-${severidad}${sufijo}`
}
