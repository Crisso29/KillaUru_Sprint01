// T043 — filtrosAConsulta: traduce FiltrosMapa → URLSearchParams para la API (R-011)
import type { FiltrosMapa } from './filtros'

const MS_24H = 86_400_000
const MS_7D = 7 * MS_24H
const MS_30D = 30 * MS_24H

export function filtrosAConsulta(filtros: FiltrosMapa, ahora: Date): URLSearchParams {
  const p = new URLSearchParams()

  p.set('estado', filtros.estados.join(','))

  if (filtros.tipos.length === 1) {
    p.set('tipo', filtros.tipos[0])
  }

  if (filtros.severidades.length < 5) {
    p.set('severidad_min', String(Math.min(...filtros.severidades)))
  }

  const offset =
    filtros.periodo === '24h' ? MS_24H :
    filtros.periodo === '7d'  ? MS_7D  : MS_30D

  p.set('desde', new Date(ahora.getTime() - offset).toISOString())

  return p
}
