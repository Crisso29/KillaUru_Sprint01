// T046 — PanelLateral: pestañas Detalle / Lista·N (FR-024)
import { useState, useEffect } from 'react'
import { Pestanas } from '@/compartido/ui'
import type { AnomaliaFeature } from '@/api/generado/002-mapa'
import { ListaAnomalias } from './ListaAnomalias'
import { DetalleAnomalia } from './DetalleAnomalia'
import { TEXTOS_MAPA } from '../textos'

interface Props {
  features: AnomaliaFeature[]
  total: number
  anomaliaId: string | null
  onSeleccionar: (id: string) => void
  onCerrarDetalle: () => void
}

export function PanelLateral({ features, total, anomaliaId, onSeleccionar, onCerrarDetalle }: Props) {
  const [activa, setActiva] = useState(anomaliaId ? 'detalle' : 'lista')

  useEffect(() => {
    if (anomaliaId) setActiva('detalle')
  }, [anomaliaId])

  const pestanas = [
    { id: 'detalle', etiqueta: TEXTOS_MAPA.tabDetalle },
    { id: 'lista', etiqueta: `${TEXTOS_MAPA.tabLista} · ${features.length}` },
  ]

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
      <Pestanas pestanas={pestanas} activa={activa} onChange={setActiva} />
      {activa === 'detalle' && (
        <div data-testid="detalle-anomalia" style={{ flex: 1, overflowY: 'auto' }}>
          {anomaliaId
            ? <DetalleAnomalia id={anomaliaId} onCerrar={onCerrarDetalle} />
            : <p style={{ padding: 'var(--s-21)', color: 'var(--ink-3)', fontSize: 'var(--t-13)' }}>
                Selecciona una anomalía del mapa o la lista.
              </p>}
        </div>
      )}
      {activa === 'lista' && (
        <ListaAnomalias
          features={features}
          total={total}
          idSeleccionada={anomaliaId}
          onSeleccionar={onSeleccionar}
        />
      )}
    </div>
  )
}
