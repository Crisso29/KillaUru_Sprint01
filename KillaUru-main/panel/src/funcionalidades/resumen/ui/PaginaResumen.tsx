// T076 — PaginaResumen: layout principal del Resumen (FR-015…FR-020)
import { useState } from 'react'
import { useIndicadores } from '../api/useIndicadores'
import { useTransiciones } from '../api/useTransiciones'
import { FranjaIndicadores } from './FranjaIndicadores'
import { GraficoEventos } from './GraficoEventos'
import { UltimosCambios } from './UltimosCambios'
import { TEXTOS_RESUMEN } from '../textos'

interface Props {
  porRevisar?: unknown[]
  pendientes?: unknown[]
  requiereAtencionSlot?: React.ReactNode
  franjaSlot?: React.ReactNode
}

export function PaginaResumen({ requiereAtencionSlot, franjaSlot }: Props) {
  const [dias, setDias] = useState<7 | 14 | 30>(14)
  const { data: resumen, isLoading, isError } = useIndicadores(dias)
  const { data: transiciones = [], isError: errorTransiciones } = useTransiciones(5)

  if (isLoading) {
    return (
      <main aria-label="Resumen">
        <p>{TEXTOS_RESUMEN.cargando.indicadores}</p>
      </main>
    )
  }

  if (isError || !resumen) {
    return (
      <main aria-label="Resumen">
        <p role="alert">{TEXTOS_RESUMEN.error.indicadores}</p>
      </main>
    )
  }

  return (
    <main aria-label="Resumen">
      <div>
        <FranjaIndicadores indicadores={resumen.indicadores} />
        <a
          href={resumen.urlPanelesTecnicos}
          target="_blank"
          rel="noopener noreferrer"
          aria-label={`${TEXTOS_RESUMEN.panelesTecnicos} (abre en otra pestaña)`}
        >
          {TEXTOS_RESUMEN.panelesTecnicos}
        </a>
      </div>
      <GraficoEventos
        datos={resumen.eventosPorDia}
        dias={dias}
        onCambioDias={setDias}
      />
      {requiereAtencionSlot}
      {franjaSlot}
      {errorTransiciones ? (
        <p role="alert">{TEXTOS_RESUMEN.error.ultimosCambios}</p>
      ) : (
        <UltimosCambios transiciones={transiciones} />
      )}
    </main>
  )
}
