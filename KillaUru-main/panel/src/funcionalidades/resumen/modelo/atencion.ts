// T074 — contarTemas y primerPendiente (data-model §5, FR-018)
import type { ElementoPendiente } from '@/api/dtos/003-anomalias'

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

/**
 * La pendiente más urgente, como «A-1061 expira en 18 h» del prototipo: la que vence antes (no la primera de la lista).
 * Si el plazo ya pasó (el trabajo que la expira aún no corrió), dice «menos de 1 h» en vez de horas negativas.
 */
export function primerPendiente(
  pendientes: ElementoPendiente[],
  ahora: Date,
): string | null {
  if (pendientes.length === 0) return null
  const vence = (pendiente: ElementoPendiente) => new Date(pendiente.venceUtc).getTime()
  const primero = pendientes.reduce((actual, otro) => (vence(otro) < vence(actual) ? otro : actual))
  const horas = Math.ceil((vence(primero) - ahora.getTime()) / 3_600_000)
  return `${primero.id.slice(0, 8)} expira en ${horas >= 1 ? horas : 'menos de 1'} h`
}
