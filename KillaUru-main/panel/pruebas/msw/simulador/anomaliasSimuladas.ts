// Anomalías del modo simulado (demo y E2E) con estado en memoria: el mapa filtra como el backend, el detalle existe
// para toda anomalía y la moderación sigue la tabla RN-07 de `backend/.../Domain/TablaModeracion.cs`.
import datosAnomalias from '../datos/anomalias.json'
import plantillaDetalle from '../datos/detalle_anomalia.json'
import datosPorRevisar from '../datos/porRevisar.json'
import type { DetalleAnomalia } from '@/api/dtos/003-anomalias'

type Anomalia = (typeof datosAnomalias.features)[number]
type Respuesta = { estado: 200; cuerpo: Detalle } | { estado: 404 | 409; cuerpo: { codigo: string } }
/** La forma del contrato (003): los JSON de ejemplo se leen con tipos amplios (`string`), no con sus uniones. */
type Detalle = DetalleAnomalia

/** Destino de cada acción según el estado de origen (RN-07). MantenerDescartada exige además «por revisar». */
const TABLA_RN07: Record<string, { desde: string[]; hacia: string }> = {
  MarcarReparada: { desde: ['Activa', 'PendienteConfirmacion'], hacia: 'Reparada' },
  Descartar: { desde: ['Activa', 'PendienteConfirmacion', 'Expirada'], hacia: 'Descartada' },
  Reabrir: { desde: ['Reparada', 'Descartada'], hacia: 'Activa' },
  MantenerDescartada: { desde: ['Descartada'], hacia: 'Descartada' },
}

/** Descartadas que algún vehículo volvió a reportar: la lista «Por revisar» del simulador. */
const POR_REVISAR_INICIAL = datosPorRevisar.map((elemento) => elemento.id)

const LISTA = (params: URLSearchParams, nombre: string) => params.get(nombre)?.split(',').filter(Boolean) ?? []

export function crearSimulador() {
  const anomalias: Anomalia[] = structuredClone(datosAnomalias.features)
  const porRevisar = new Set(POR_REVISAR_INICIAL)
  const buscar = (id: string) => anomalias.find((anomalia) => anomalia.properties.id === id)

  const detalle = (id: string): Detalle | null => {
    const anomalia = buscar(id)
    if (!anomalia) return null
    const { severidad, ...propiedades } = anomalia.properties
    return { ...plantillaDetalle, ...propiedades, severidadMostrada: severidad, porRevisar: porRevisar.has(id) } as Detalle
  }

  return {
    listar: (params: URLSearchParams) => ({ ...datosAnomalias, features: anomalias.filter((anomalia) => coincide(anomalia, params)) }),
    detalle,
    /** «Por revisar» sin las que el administrador ya decidió. */
    porRevisar: () => datosPorRevisar.filter((elemento) => porRevisar.has(elemento.id)),
    moderar: (id: string, accion: string): Respuesta => {
      const anomalia = buscar(id)
      if (!anomalia) return { estado: 404, cuerpo: { codigo: 'no_encontrado' } }
      const regla = TABLA_RN07[accion]
      const permitida = regla?.desde.includes(anomalia.properties.estado) && (accion !== 'MantenerDescartada' || porRevisar.has(id))
      if (!regla || !permitida) return { estado: 409, cuerpo: { codigo: 'transicion_no_permitida' } }
      anomalia.properties.estado = regla.hacia
      porRevisar.delete(id) // toda decisión del administrador cierra la revisión pendiente
      return { estado: 200, cuerpo: detalle(id)! }
    },
  }
}

export type SimuladorAnomalias = ReturnType<typeof crearSimulador>

/** Filtros del contrato 002: estado, tipo y severidad separados por comas; ausente = sin filtro. */
function coincide({ properties }: Anomalia, params: URLSearchParams): boolean {
  const [estados, tipos, severidades] = [LISTA(params, 'estado'), LISTA(params, 'tipo'), LISTA(params, 'severidad').map(Number)]
  return (!estados.length || estados.includes(properties.estado))
    && (!tipos.length || tipos.includes(properties.tipo))
    && (!severidades.length || severidades.includes(properties.severidad))
}
