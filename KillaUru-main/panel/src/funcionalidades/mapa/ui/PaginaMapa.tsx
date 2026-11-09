// T048 — PaginaMapa: filtros y selección en la dirección (FR-022, FR-026)
import { useMemo } from 'react'
import { useSearchParams } from 'react-router'
import { EstadoError } from '@/compartido/ui'
import { relojReal } from '@/compartido/reloj'
import type { ErrorApi } from '@/api/problema'
import { filtrosDeParams, filtrosAParams, esGrupoVacio } from '../modelo/filtros'
import { filtrosAConsulta } from '../modelo/peticion'
import { useMapaAdmin } from '../api/useMapaAdmin'
import { LienzoMapa } from './LienzoMapa'
import { PanelLateral } from './PanelLateral'
import { BarraFiltros } from './BarraFiltros'

export function PaginaMapa() {
  const [params, setParams] = useSearchParams()

  const filtros = useMemo(() => filtrosDeParams(params), [params])
  const anomaliaId = params.get('anomalia')

  const consulta = useMemo(() => {
    if (esGrupoVacio(filtros)) return null
    return filtrosAConsulta(filtros, relojReal.ahora())
  }, [filtros])

  const { data, isLoading, error, refetch } = useMapaAdmin(consulta)
  const features = data?.features ?? []
  const total = features.length

  function seleccionar(id: string) {
    const next = new URLSearchParams(params)
    next.set('anomalia', id)
    setParams(next)
  }

  function cerrarDetalle() {
    const next = new URLSearchParams(params)
    next.delete('anomalia')
    setParams(next)
  }

  function cambiarFiltros(nuevos: typeof filtros) {
    const next = filtrosAParams(nuevos)
    if (anomaliaId) next.set('anomalia', anomaliaId)
    setParams(next)
  }

  if (!isLoading && error) {
    return <EstadoError error={error as ErrorApi} onReintentar={() => void refetch()} />
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <BarraFiltros filtros={filtros} onChange={cambiarFiltros} />
      <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
        <div style={{ flex: 1, position: 'relative' }}>
          <LienzoMapa datos={data} anomaliaId={anomaliaId} onSeleccionar={seleccionar} />
        </div>
        <div style={{ width: 320, borderLeft: '1px solid var(--border)', overflowY: 'auto' }}>
          <PanelLateral
            features={features}
            total={total}
            anomaliaId={anomaliaId}
            onSeleccionar={seleccionar}
            onCerrarDetalle={cerrarDetalle}
          />
        </div>
      </div>
    </div>
  )
}
