// T043 — busqueda: interpreta texto libre para navegación rápida (FR-040, R-028)

export interface AnomaliaEnMapa {
  id: string
  kmVia: number
}

export type IntentoBusqueda =
  | { tipo: 'id'; id: string }
  | { tipo: 'id-prefijo'; prefijo: string }
  | { tipo: 'km'; km: number }
  | { tipo: 'correo'; correo: string }
  | { tipo: 'sin_resultados' }

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
const PREFIJO_RE = /^[0-9a-f]{8}$/i
const KM_RE = /^\d+,\d+$/

export function parsearBusqueda(texto: string): IntentoBusqueda {
  const limpio = texto.trim()
  if (!limpio) return { tipo: 'sin_resultados' }
  if (UUID_RE.test(limpio)) return { tipo: 'id', id: limpio }
  if (PREFIJO_RE.test(limpio)) return { tipo: 'id-prefijo', prefijo: limpio }
  if (KM_RE.test(limpio)) return { tipo: 'km', km: parseFloat(limpio.replace(',', '.')) }
  if (limpio.includes('@')) return { tipo: 'correo', correo: limpio }
  return { tipo: 'sin_resultados' }
}

export function masProxima(km: number, anomalias: AnomaliaEnMapa[]): AnomaliaEnMapa | null {
  if (!anomalias.length) return null
  return anomalias.reduce((mejor, actual) =>
    Math.abs(actual.kmVia - km) < Math.abs(mejor.kmVia - km) ? actual : mejor,
  )
}
