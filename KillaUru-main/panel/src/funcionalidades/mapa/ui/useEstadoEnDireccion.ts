// T048 — filtros y anomalía elegida viven en la dirección (FR-026): un enlace copiado abre la misma vista
import { useMemo } from 'react'
import { useSearchParams } from 'react-router'
import { relojReal } from '@/compartido/reloj'
import { filtrosDeParams, filtrosAParams, esGrupoVacio, type FiltrosMapa } from '../modelo/filtros'
import { filtrosAConsulta } from '../modelo/peticion'

export function useEstadoEnDireccion() {
  const [params, setParams] = useSearchParams()
  const filtros = useMemo(() => filtrosDeParams(params), [params])
  const anomaliaId = params.get('anomalia')
  // Un grupo de filtros vacío (p. ej. ningún estado marcado) no pide nada al servidor
  const consulta = useMemo(() => (esGrupoVacio(filtros) ? null : filtrosAConsulta(filtros, relojReal.ahora())), [filtros])
  const conAnomalia = (id: string | null) => {
    const siguientes = new URLSearchParams(params)
    if (id) siguientes.set('anomalia', id)
    else siguientes.delete('anomalia')
    setParams(siguientes)
  }
  const cambiarFiltros = (nuevos: FiltrosMapa) => {
    const siguientes = filtrosAParams(nuevos)
    if (anomaliaId) siguientes.set('anomalia', anomaliaId)
    setParams(siguientes)
  }
  return { filtros, consulta, anomaliaId, seleccionar: conAnomalia, cerrarDetalle: () => conAnomalia(null), cambiarFiltros }
}
