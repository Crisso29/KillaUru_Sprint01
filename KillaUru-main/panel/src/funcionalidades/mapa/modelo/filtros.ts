// T043 — FiltrosMapa: defecto, URL ↔ estado, grupo vacío (data-model §4)
import type { EstadoAnomalia, TipoAnomalia } from '@/api/generado/002-mapa'

export type Periodo = '24h' | '7d' | '30d'

export interface FiltrosMapa {
  estados: EstadoAnomalia[]
  tipos: TipoAnomalia[]
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
  const estados: EstadoAnomalia[] = estadoRaw
    ? estadoRaw.split(',').filter((e): e is EstadoAnomalia => ESTADOS_VALIDOS.includes(e as EstadoAnomalia))
    : ['Activa', 'PendienteConfirmacion']

  const tipoRaw = params.get('tipo')
  const tipos: TipoAnomalia[] = tipoRaw
    ? tipoRaw.split(',').filter((t): t is TipoAnomalia => TIPOS_VALIDOS.includes(t as TipoAnomalia))
    : ['Bache', 'Hundimiento']

  const sevRaw = params.get('severidad')
  const severidades = sevRaw
    ? sevRaw.split(',').map(Number).filter(n => SEVERIDADES_VALIDAS.includes(n))
    : [1, 2, 3, 4, 5]

  const periodoRaw = params.get('periodo')
  const periodo: Periodo = PERIODOS_VALIDOS.includes(periodoRaw as Periodo)
    ? (periodoRaw as Periodo)
    : '7d'

  return { estados, tipos, severidades, periodo }
}

export function filtrosAParams(f: FiltrosMapa): URLSearchParams {
  const p = new URLSearchParams()
  p.set('estado', f.estados.join(','))
  p.set('tipo', f.tipos.join(','))
  p.set('severidad', f.severidades.join(','))
  p.set('periodo', f.periodo)
  return p
}

export function esGrupoVacio(f: FiltrosMapa): boolean {
  return f.estados.length === 0 || f.tipos.length === 0 || f.severidades.length === 0
}
