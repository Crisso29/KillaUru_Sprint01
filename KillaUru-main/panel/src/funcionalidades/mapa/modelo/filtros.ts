// T043 — FiltrosMapa: defecto, URL ↔ estado, grupo vacío (data-model §4)
import type { EstadoAnomalia, TipoAnomalia } from '@/api/dtos/002-mapa'

export type Periodo = '24h' | '7d' | '30d'

export interface FiltrosMapa {
  estados: readonly EstadoAnomalia[]
  tipos: readonly TipoAnomalia[]
  severidades: number[]
  periodo: Periodo
}

const ESTADOS_VALIDOS: EstadoAnomalia[] = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada']
const TIPOS_VALIDOS: TipoAnomalia[] = ['Bache', 'Hundimiento']
const SEVERIDADES_VALIDAS = [1, 2, 3, 4, 5]
const PERIODOS_VALIDOS: Periodo[] = ['24h', '7d', '30d']

export function filtrosDefecto(): FiltrosMapa {
  return {
    estados: ['Activa', 'PendienteConfirmacion'],
    tipos: ['Bache', 'Hundimiento'],
    severidades: [1, 2, 3, 4, 5],
    periodo: '7d',
  }
}

export function filtrosDeParams(params: URLSearchParams): FiltrosMapa {
  const estadoRaw = params.get('estado')
  const estados: readonly EstadoAnomalia[] = estadoRaw
    ? estadoRaw.split(',').filter((estado): estado is EstadoAnomalia => ESTADOS_VALIDOS.includes(estado as EstadoAnomalia))
    : ['Activa', 'PendienteConfirmacion']

  const tipoRaw = params.get('tipo')
  const tipos: readonly TipoAnomalia[] = tipoRaw
    ? tipoRaw.split(',').filter((tipo): tipo is TipoAnomalia => TIPOS_VALIDOS.includes(tipo as TipoAnomalia))
    : ['Bache', 'Hundimiento']

  const sevRaw = params.get('severidad')
  const severidades = sevRaw
    ? sevRaw.split(',').map(Number).filter(nivel => SEVERIDADES_VALIDAS.includes(nivel))
    : [1, 2, 3, 4, 5]

  const periodoRaw = params.get('periodo')
  const periodo: Periodo = PERIODOS_VALIDOS.includes(periodoRaw as Periodo)
    ? (periodoRaw as Periodo)
    : '7d'

  return { estados, tipos, severidades, periodo }
}

export function filtrosAParams(filtros: FiltrosMapa): URLSearchParams {
  const parametros = new URLSearchParams()
  parametros.set('estado', filtros.estados.join(','))
  parametros.set('tipo', filtros.tipos.join(','))
  parametros.set('severidad', filtros.severidades.join(','))
  parametros.set('periodo', filtros.periodo)
  return parametros
}

export function esGrupoVacio(filtros: FiltrosMapa): boolean {
  return filtros.estados.length === 0 || filtros.tipos.length === 0 || filtros.severidades.length === 0
}
