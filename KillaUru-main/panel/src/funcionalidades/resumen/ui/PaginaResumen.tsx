// T076 — PaginaResumen: layout del Resumen según el prototipo (FR-015…FR-020): encabezado, indicadores,
// gráfico y «Requiere atención» en proporción áurea (.g61), franja del corredor y últimos cambios.
import { useState } from 'react'
import { useIndicadores } from '../api/useIndicadores'
import { useTransiciones } from '../api/useTransiciones'
import { FranjaIndicadores } from './FranjaIndicadores'
import { GraficoEventos } from './GraficoEventos'
import { UltimosCambios } from './UltimosCambios'
import { TEXTOS_RESUMEN } from '../textos'
import { Esqueleto, Icono } from '@/compartido/ui'
import { formatearDiaLargoLima } from '@/compartido/formato/fechas'

interface Props {
  requiereAtencionSlot?: React.ReactNode
  franjaSlot?: React.ReactNode
  /** Fecha del subtítulo; por defecto hoy (se inyecta en pruebas). */
  hoy?: Date
}

export function PaginaResumen({ requiereAtencionSlot, franjaSlot, hoy = new Date() }: Props) {
  const [dias, setDias] = useState<7 | 14 | 30>(14)
  const { data: resumen, isLoading, isError } = useIndicadores(dias)
  const { data: transiciones = [], isError: errorTransiciones } = useTransiciones(5)

  const encabezado = (
    <div className="pg-h">
      <div>
        <h2>Resumen</h2>
        <p>{formatearDiaLargoLima(hoy.toISOString())} · hora de Lima · periodo {dias} d</p>
      </div>
      {resumen && (
        <div className="acts">
          <a className="ob" href={resumen.urlPanelesTecnicos} target="_blank" rel="noopener noreferrer"
            aria-label={`${TEXTOS_RESUMEN.panelesTecnicos} (abre en otra pestaña)`}>
            <Icono nombre="ext" tamano={14} />{TEXTOS_RESUMEN.panelesTecnicos}
          </a>
        </div>
      )}
    </div>
  )

  if (isLoading) {
    return (
      <section aria-label="Resumen" aria-busy="true">
        {encabezado}
        <p className="sr-only">{TEXTOS_RESUMEN.cargando.indicadores}</p>
        <Esqueleto filas={3} alto={89} />
      </section>
    )
  }

  if (isError || !resumen) {
    return (
      <section aria-label="Resumen">
        {encabezado}
        <p role="alert" className="note bad">{TEXTOS_RESUMEN.error.indicadores}</p>
      </section>
    )
  }

  return (
    <section aria-label="Resumen" style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-21)' }}>
      {encabezado}
      <FranjaIndicadores indicadores={resumen.indicadores} />
      <div className="g61">
        <GraficoEventos datos={resumen.eventosPorDia} dias={dias} onCambioDias={setDias} />
        {requiereAtencionSlot}
      </div>
      {franjaSlot}
      {errorTransiciones ? (
        <p role="alert" className="note bad">{TEXTOS_RESUMEN.error.ultimosCambios}</p>
      ) : (
        <UltimosCambios transiciones={transiciones} />
      )}
    </section>
  )
}
