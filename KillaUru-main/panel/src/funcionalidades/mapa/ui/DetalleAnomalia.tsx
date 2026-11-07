// T047 / T067 — DetalleAnomalia con AccionesModeracion
import { useState } from 'react'
import { Esqueleto, EstadoError, Boton } from '@/compartido/ui'
import type { ErrorApi } from '@/api/problema'
import { useDetalleAnomalia } from '../api/useDetalleAnomalia'
import { DatosAnomalia } from './DatosAnomalia'
import { ReportesPorDia } from './ReportesPorDia'
import { Historial } from './Historial'
import { TEXTOS_DETALLE } from '../textos'
import { AccionesModeracion } from '@/funcionalidades/moderacion'
import type { DetalleAnomalia as TDetalle } from '@/api/generado/003-anomalias'

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

  return (
    <article style={{ padding: 'var(--s-13)', overflowY: 'auto' }}>
      {onCerrar && (
        <Boton variante="secundario" onClick={onCerrar} style={{ marginBottom: 'var(--s-13)' }}>
          ← Volver
        </Boton>
      )}
      <DatosAnomalia detalle={data} />
      <ReportesPorDia datos={data.reportesPorDia} />
      <Historial historial={data.historial} />
      <AccionesModeracion detalle={data} onModerado={setLocalData} />
    </article>
  )
}
