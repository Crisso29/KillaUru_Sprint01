// T043 — simbolos: marca por estado × severidad para los íconos del mapa (FR-023, prototipo «Nunca solo color»)

/** Marca que distingue el estado sobre el círculo de severidad, sin depender del color (FR-023). */
export type MarcaEstado = 'lleno' | 'punteado' | 'anillo' | 'check' | 'cruz'

export const ESTADOS_MAPA = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada'] as const
export const SEVERIDADES = [1, 2, 3, 4, 5] as const

const MARCA_POR_ESTADO: Record<string, MarcaEstado> = {
  Activa: 'lleno',
  PendienteConfirmacion: 'punteado',
  Expirada: 'anillo',
  Reparada: 'check',
  Descartada: 'cruz',
}

export function marcaParaEstado(estado: string): MarcaEstado {
  return MARCA_POR_ESTADO[estado] ?? 'lleno'
}

/** FR-023: solo las activas de severidad 4 y 5 llevan anillo exterior (piden frenar). */
export function tieneAnillo(estado: string, severidad: number): boolean {
  return estado === 'Activa' && severidad >= 4
}

/** Nombre de la imagen en MapLibre: `lleno-4-anillo`, `punteado-2`… (25 combinaciones). */
export function claveIcono(estado: string, severidad: number): string {
  const sufijo = tieneAnillo(estado, severidad) ? '-anillo' : ''
  return `${marcaParaEstado(estado)}-${severidad}${sufijo}`
}
