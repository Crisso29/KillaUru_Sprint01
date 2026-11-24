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
  const { data: consultado, isLoading, error } = useDetalleAnomalia(id)
  // Tras moderar se muestra al instante lo que devolvió el servidor, sin esperar a que se vuelva a pedir el detalle
  const [moderado, setModerado] = useState<TDetalle | null>(null)
  const detalle = moderado ?? consultado
  if (isLoading) return <Esqueleto alto={300} />
  if (error) return <ErrorDetalle error={error as ErrorApi} onCerrar={onCerrar} />
  if (!detalle) return null
  // Prototipo: encabezado `.dhead` (severidad, id, tipo y estado), secciones y acciones fijas al pie (`.rp-foot`).
  return (
    <article style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <div className="rp-body">
        <EncabezadoDetalle detalle={detalle} />
        <DatosAnomalia detalle={detalle} />
        <ReportesPorDia datos={detalle.reportesPorDia} />
        <Historial historial={detalle.historial} />
      </div>
      <PieDetalle detalle={detalle} onModerado={setModerado} onCerrar={onCerrar} />
    </article>
  )
}

/** Un id que ya no existe (enlace viejo) se explica con texto propio; los demás errores usan el estado común. */
function ErrorDetalle({ error, onCerrar }: { error: ErrorApi; onCerrar?: () => void }) {
  if (error.codigo !== 'no_encontrado') return <EstadoError error={error} />
  return (
    <div style={{ padding: 'var(--s-21)' }}>
      <p>{TEXTOS_DETALLE.noExiste}</p>
      {onCerrar && <Boton variante="secundario" onClick={onCerrar}>Volver</Boton>}
    </div>
  )
}

function EncabezadoDetalle({ detalle }: { detalle: TDetalle }) {
  return (
    <header className="dsec">
      <div className="dhead">
        <Severidad nivel={detalle.severidadMostrada} tamano="lg" />
        <div style={{ flex: 1 }}>
          <span className="mono hint">{detalle.id.slice(0, 8)}</span>
          <h3>{detalle.tipo} · {NOMBRE_SEVERIDAD[detalle.severidadMostrada]?.toLowerCase()}</h3>
          <p>{DESCRIPCION_SEVERIDAD[detalle.severidadMostrada]}</p>
        </div>
        <InsigniaEstado estado={detalle.estado} />
      </div>
    </header>
  )
}

interface PropsPie { detalle: TDetalle; onModerado: (detalle: TDetalle) => void; onCerrar?: () => void }

/** Acciones fijas al pie (`.rp-foot`): moderar según el estado y cerrar el detalle. */
function PieDetalle({ detalle, onModerado, onCerrar }: PropsPie) {
  return (
    <div className="rp-foot">
      <AccionesModeracion detalle={detalle} onModerado={onModerado} />
      {onCerrar && <Boton variante="fantasma" onClick={onCerrar} className="sp">Cerrar</Boton>}
    </div>
  )
}
