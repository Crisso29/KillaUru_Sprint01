// T074 — contarTemas y primerPendiente (data-model §5, FR-018)
import type { ElementoPendiente } from '@/api/generado/003-anomalias'

interface EstadoRevisionMinimo {
  venceUtc: string
  administradores: unknown[]
}

const UMBRAL_REVISION_DIAS = 15

export function contarTemas(
  cantidadPorRevisar: number,
  cantidadPendientes: number,
  revision: EstadoRevisionMinimo,
  ahora: Date,
): number {
  const diasRestantes = Math.floor(
    (new Date(revision.venceUtc).getTime() - ahora.getTime()) / (1000 * 60 * 60 * 24),
  )
  const sumaRevision = diasRestantes <= UMBRAL_REVISION_DIAS ? 1 : 0
  return cantidadPorRevisar + cantidadPendientes + sumaRevision
}

export function primerPendiente(
  pendientes: ElementoPendiente[],
  ahora: Date,
): string | null {
  if (pendientes.length === 0) return null
  const primero = pendientes[0]
  const idCorto = primero.id.slice(0, 8)
  const horasRestantes = Math.ceil(
    (new Date(primero.venceUtc).getTime() - ahora.getTime()) / (1000 * 60 * 60),
  )
  return `${idCorto} expira en ${horasRestantes} h`
}
