// T047 — DetalleAnomalia: panel completo con DatosAnomalia, ReportesPorDia e Historial
import { Esqueleto, EstadoError, Boton } from '@/compartido/ui'
import { mensajeDeError } from '@/compartido/textos/errores'
import type { ErrorApi } from '@/api/problema'
import { useDetalleAnomalia } from '../api/useDetalleAnomalia'
import { DatosAnomalia } from './DatosAnomalia'
import { ReportesPorDia } from './ReportesPorDia'
import { Historial } from './Historial'
import { TEXTOS_DETALLE } from '../textos'

interface Props {
  id: string
  onCerrar?: () => void
}

export function DetalleAnomalia({ id, onCerrar }: Props) {
  const { data, isLoading, error } = useDetalleAnomalia(id)

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
    </article>
  )
}
