// T066 — acciones del administrador según RN-07
import type { AccionModeracion, EstadoAnomalia } from '@/api/dtos/003-anomalias'

const MAPA_ACCIONES: Record<EstadoAnomalia, AccionModeracion[]> = {
  Activa: ['MarcarReparada', 'Descartar'],
  PendienteConfirmacion: ['MarcarReparada', 'Descartar'],
  Expirada: ['Descartar'],
  Reparada: ['Reabrir'],
  Descartada: ['Reabrir'],
}

const ESTADOS_POR_ACCION: Record<AccionModeracion, EstadoAnomalia[]> = {
  MarcarReparada: ['Activa', 'PendienteConfirmacion'],
  Descartar: ['Activa', 'PendienteConfirmacion', 'Expirada'],
  Reabrir: ['Reparada', 'Descartada'],
  MantenerDescartada: ['Descartada'],
}

export function accionesDisponibles(estado: EstadoAnomalia, porRevisar: boolean): AccionModeracion[] {
  const base = MAPA_ACCIONES[estado] ?? []
  if (estado === 'Descartada' && porRevisar) return [...base, 'MantenerDescartada']
  return base
}

export function esAccionDisponible(accion: AccionModeracion, estado: EstadoAnomalia, porRevisar: boolean): boolean {
  if (accion === 'MantenerDescartada') return estado === 'Descartada' && porRevisar
  return accionesDisponibles(estado, porRevisar).includes(accion)
}

export function textoNoDisponible(accion: AccionModeracion, estado: EstadoAnomalia): string {
  const soloDesde = ESTADOS_POR_ACCION[accion].join(', ')
  return `No disponible desde ${estado}. Solo desde: ${soloDesde}.`
}
