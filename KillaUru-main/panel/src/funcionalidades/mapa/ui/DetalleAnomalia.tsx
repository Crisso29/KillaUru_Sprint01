// T047 / T067 — DetalleAnomalia con AccionesModeracion
import { useState } from 'react'
import { Esqueleto, EstadoError, Boton, Severidad, NOMBRE_SEVERIDAD, InsigniaEstado } from '@/compartido/ui'
import type { ErrorApi } from '@/api/problema'
import { useDetalleAnomalia } from '../api/useDetalleAnomalia'
import { DatosAnomalia } from './DatosAnomalia'
import { ReportesPorDia } from './ReportesPorDia'
import { Historial } from './Historial'
import { TEXTOS_DETALLE } from '../textos'
import { AccionesModeracion } from '@/funcionalidades/moderacion'
import type { DetalleAnomalia as TDetalle } from '@/api/dtos/003-anomalias'

/** Qué significa cada nivel para quien maneja (prototipo `SEV`). */
const DESCRIPCION_SEVERIDAD = ['', 'Imperceptible a velocidad normal', 'Se siente, no exige frenar',
  'Conviene reducir la velocidad', 'Hay que frenar; riesgo de daño al vehículo', 'Riesgo de pérdida de control']

interface Props {
  id: string
  onCerrar?: () => void
}

export function DetalleAnomalia({ id, onCerrar }: Props) {
  const { data: queryData, isLoading, error } = useDetalleAnomalia(id)
  const [localData, setLocalData] = useState<TDetalle | null>(null)
  const data = localData ?? queryData

  if (isLoading) return <Esqueleto alto={300} />

  if (error) {
    const e = error as ErrorApi
    if (e.codigo === 'no_encontrado') {
      return (
        <div style={{ padding: 'var(--s-21)' }}>
          <p>{TEXTOS_DETALLE.noExiste}</p>
          {onCerrar && <Boton variante="secundario" onClick={onCerrar}>Volver</Boton>}
        </div>
      )
    }
    return <EstadoError error={e} />
  }

  if (!data) return null

  // Prototipo: encabezado `.dhead` (severidad, id, tipo y estado), secciones y acciones fijas al pie (`.rp-foot`).
  return (
    <article style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <div className="rp-body">
        <header className="dsec">
          <div className="dhead">
            <Severidad nivel={data.severidadMostrada} tamano="lg" />
            <div style={{ flex: 1 }}>
              <span className="mono hint">{data.id.slice(0, 8)}</span>
              <h3>{data.tipo} · {NOMBRE_SEVERIDAD[data.severidadMostrada]?.toLowerCase()}</h3>
              <p>{DESCRIPCION_SEVERIDAD[data.severidadMostrada]}</p>
            </div>
            <InsigniaEstado estado={data.estado} />
          </div>
        </header>
        <DatosAnomalia detalle={data} />
        <ReportesPorDia datos={data.reportesPorDia} />
        <Historial historial={data.historial} />
      </div>
      <div className="rp-foot">
        <AccionesModeracion detalle={data} onModerado={setLocalData} />
        {onCerrar && <Boton variante="fantasma" onClick={onCerrar} className="sp">Cerrar</Boton>}
      </div>
    </article>
  )
}
