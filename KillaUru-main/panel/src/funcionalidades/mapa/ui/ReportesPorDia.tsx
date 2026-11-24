// T047 — ReportesPorDia: barras de los últimos 15 días y tabla para lectores (FR-025; prototipo)
import { TablaOculta } from '@/compartido/ui'
import type { ReportePorDia } from '@/api/dtos/002-mapa'
import { TEXTOS_DETALLE } from '../textos'

interface Props { datos: ReportePorDia[] }

const ANCHO = 420, ALTO = 64

export function ReportesPorDia({ datos }: Props) {
  if (!datos.length) return null
  const total = datos.reduce((suma, fila) => suma + fila.cantidad, 0)
  return (
    <section className="dsec" aria-label={TEXTOS_DETALLE.reportesPorDia}>
      <h4>{TEXTOS_DETALLE.reportesPorDia} · últimos {datos.length} días</h4>
      <figure role="img" aria-label={`${TEXTOS_DETALLE.reportesPorDia}: ${total} total`} style={{ margin: 0 }}>
        <Barras datos={datos} />
        <TablaOculta caption={TEXTOS_DETALLE.reportesPorDia}>
          <thead><tr><th>Día</th><th>Cantidad</th></tr></thead>
          <tbody>{datos.map((fila) => <tr key={fila.dia}><td>{fila.dia}</td><td>{fila.cantidad}</td></tr>)}</tbody>
        </TablaOculta>
      </figure>
    </section>
  )
}

/** Una barra por día, escalada al día con más reportes; la tabla oculta da los mismos datos a los lectores de pantalla. */
function Barras({ datos }: Props) {
  const maximo = Math.max(...datos.map((fila) => fila.cantidad), 1)
  const ancho = ANCHO / datos.length
  return (
    <svg viewBox={`0 0 ${ANCHO} ${ALTO}`} preserveAspectRatio="none" style={{ width: '100%', height: ALTO }} aria-hidden="true">
      {datos.map((fila, i) => {
        const alto = (fila.cantidad / maximo) * (ALTO - 2)
        return <rect key={fila.dia} x={i * ancho + 2} y={ALTO - alto} width={ancho - 4} height={alto} fill="var(--killa)" opacity=".8"><title>{`${fila.dia}: ${fila.cantidad}`}</title></rect>
      })}
    </svg>
  )
}
