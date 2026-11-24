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
  const encabezado = <Encabezado hoy={hoy} dias={dias} urlPaneles={resumen?.urlPanelesTecnicos} />
  if (isLoading || isError || !resumen) return <ResumenSinDatos encabezado={encabezado} cargando={isLoading} />
  return (
    <section aria-label="Resumen" style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-21)' }}>
      {encabezado}
      <FranjaIndicadores indicadores={resumen.indicadores} />
      <div className="g61"><GraficoEventos datos={resumen.eventosPorDia} dias={dias} onCambioDias={setDias} />{requiereAtencionSlot}</div>
      {franjaSlot}
      <SeccionUltimosCambios />
    </section>
  )
}

/** Mientras cargan los indicadores o si fallan, el encabezado se queda (el administrador sabe dónde está). */
function ResumenSinDatos({ encabezado, cargando }: { encabezado: React.ReactNode; cargando: boolean }) {
  if (!cargando) return <section aria-label="Resumen">{encabezado}<p role="alert" className="note bad">{TEXTOS_RESUMEN.error.indicadores}</p></section>
  return (
    <section aria-label="Resumen" aria-busy="true">
      {encabezado}
      <p className="sr-only">{TEXTOS_RESUMEN.cargando.indicadores}</p>
      <Esqueleto filas={3} alto={89} />
    </section>
  )
}

/** Si fallan los últimos cambios, el resto del Resumen sigue visible. */
function SeccionUltimosCambios() {
  const { data: transiciones = [], isError } = useTransiciones(5)
  if (isError) return <p role="alert" className="note bad">{TEXTOS_RESUMEN.error.ultimosCambios}</p>
  return <UltimosCambios transiciones={transiciones} />
}

function Encabezado({ hoy, dias, urlPaneles }: { hoy: Date; dias: number; urlPaneles?: string }) {
  return (
    <div className="pg-h">
      <div>
        <h2>Resumen</h2>
        <p>{formatearDiaLargoLima(hoy.toISOString())} · hora de Lima · periodo {dias} d</p>
      </div>
      {urlPaneles && (
        <div className="acts">
          <a className="ob" href={urlPaneles} target="_blank" rel="noopener noreferrer" aria-label={`${TEXTOS_RESUMEN.panelesTecnicos} (abre en otra pestaña)`}>
            <Icono nombre="ext" tamano={14} />{TEXTOS_RESUMEN.panelesTecnicos}
          </a>
        </div>
      )}
    </div>
  )
}
