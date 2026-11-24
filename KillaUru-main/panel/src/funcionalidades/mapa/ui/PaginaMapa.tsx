// T048 — PaginaMapa: filtros y selección en la dirección (FR-022, FR-026)
import { EstadoError } from '@/compartido/ui'
import type { ErrorApi } from '@/api/problema'
import { useMapaAdmin } from '../api/useMapaAdmin'
import { LienzoMapa } from './LienzoMapa'
import { PanelLateral } from './PanelLateral'
import { BarraFiltros } from './BarraFiltros'
import { Leyenda } from './Leyenda'
import { useEstadoEnDireccion } from './useEstadoEnDireccion'

export function PaginaMapa() {
  const { filtros, consulta, anomaliaId, seleccionar, cerrarDetalle, cambiarFiltros } = useEstadoEnDireccion()
  const { data, isLoading, error, refetch } = useMapaAdmin(consulta)
  const features = data?.features ?? []
  if (!isLoading && error) return <EstadoError error={error as ErrorApi} onReintentar={() => void refetch()} />
  // Prototipo `.mapl`: mapa (61,8 %) con filtros flotantes y leyenda, y a la derecha el panel Detalle / Lista (38,2 %).
  return (
    <div className="mapl" style={{ height: '100%' }}>
      <div style={{ position: 'relative', minHeight: 0 }}>
        <LienzoMapa datos={data} anomaliaId={anomaliaId} onSeleccionar={seleccionar} />
        <BarraFiltros filtros={filtros} onChange={cambiarFiltros} />
        <Leyenda />
      </div>
      <aside className="rp" aria-label="Detalle y lista de anomalías">
        <PanelLateral features={features} total={features.length} anomaliaId={anomaliaId}
          onSeleccionar={seleccionar} onCerrarDetalle={cerrarDetalle} />
      </aside>
    </div>
  )
}
