// T074 — franja del corredor: tramo(k) = floor(min(km, 332.999) / 10) (data-model §5, FR-019)
import type { AnomaliaFeature } from '@/api/generado/002-mapa'

const LIMITE_KM = 332.999
const LARGO_TRAMO = 10

export function calcularTramo(kmVia: number): number {
  return Math.floor(Math.min(kmVia, LIMITE_KM) / LARGO_TRAMO)
}

export interface TramoFranja {
  tramo: number
  desdeKm: number
  cantidad: number
  severidadMax: number
}

export function agruparEnFranja(anomalias: AnomaliaFeature[]): TramoFranja[] {
  const mapa = new Map<number, { cantidad: number; severidadMax: number }>()
  for (const a of anomalias) {
    const t = calcularTramo(a.properties.kmVia)
    const actual = mapa.get(t) ?? { cantidad: 0, severidadMax: 0 }
    mapa.set(t, {
      cantidad: actual.cantidad + 1,
      severidadMax: Math.max(actual.severidadMax, a.properties.severidad),
    })
  }
  return Array.from(mapa.entries())
    .map(([tramo, { cantidad, severidadMax }]) => ({
      tramo,
      desdeKm: tramo * LARGO_TRAMO,
      cantidad,
      severidadMax,
    }))
    .sort((a, b) => a.tramo - b.tramo)
}
