// T084 — conversión fecha Lima → UTC para SolicitudExportacion
import type { TipoAnomalia, EstadoAnomalia } from '@/api/generado/003-anomalias'

export interface SolicitudExportacion {
  formato: 'CSV' | 'GeoJSON'
  desdeUtc: string | null
  hastaUtc: string | null
  tipo: TipoAnomalia | null
  estados: EstadoAnomalia[]
}

// Lima = UTC-5; medianoche local = 05:00 UTC del mismo día
export function aDesdeUtc(fecha: string): string | null {
  if (!fecha) return null
  return `${fecha}T05:00:00Z`
}

// fin del día Lima = 04:59:59 UTC del día siguiente
export function aHastaUtc(fecha: string): string | null {
  if (!fecha) return null
  const [a, m, d] = fecha.split('-').map(Number)
  const sig = new Date(Date.UTC(a, m - 1, d + 1))
  const yyyy = sig.getUTCFullYear()
  const mm = String(sig.getUTCMonth() + 1).padStart(2, '0')
  const dd = String(sig.getUTCDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}T04:59:59Z`
}

export function construirSolicitud(
  formato: 'CSV' | 'GeoJSON',
  desde: string,
  hasta: string,
  tipo: string,
  estados: EstadoAnomalia[],
): SolicitudExportacion {
  return {
    formato,
    desdeUtc: aDesdeUtc(desde),
    hastaUtc: aHastaUtc(hasta),
    tipo: tipo === 'Todos' || !tipo ? null : (tipo as TipoAnomalia),
    estados,
  }
}
