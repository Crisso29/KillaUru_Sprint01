// T043 — filtrosAConsulta: traduce FiltrosMapa → URLSearchParams para la API (R-011)
import type { FiltrosMapa } from './filtros'

const MS_24H = 86_400_000
const MS_7D = 7 * MS_24H
const MS_30D = 30 * MS_24H

export function filtrosAConsulta(filtros: FiltrosMapa, ahora: Date): URLSearchParams {
  const parametros = new URLSearchParams()

  parametros.set('estado', filtros.estados.join(','))

  if (filtros.tipos.length === 1) {
    parametros.set('tipo', filtros.tipos[0]!)
  }

  // Contrato 006: lista separada por comas («2,5» no es lo mismo que «desde 2»). Con las 5 no se filtra.
  if (filtros.severidades.length < 5) {
    parametros.set('severidad', [...filtros.severidades].sort().join(','))
  }

  const offset =
    filtros.periodo === '24h' ? MS_24H :
    filtros.periodo === '7d'  ? MS_7D  : MS_30D

  parametros.set('desde', new Date(ahora.getTime() - offset).toISOString())

  return parametros
}
