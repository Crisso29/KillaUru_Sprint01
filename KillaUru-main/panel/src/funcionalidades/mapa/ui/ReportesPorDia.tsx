// T047 — ReportesPorDia: barras de los últimos 15 días y tabla para lectores (FR-025; prototipo)
import { TablaOculta } from '@/compartido/ui'
import type { ReportePorDia } from '@/api/dtos/002-mapa'
import { TEXTOS_DETALLE } from '../textos'

interface Props { datos: ReportePorDia[] }

const W = 420, H = 64

export function ReportesPorDia({ datos }: Props) {
  if (!datos.length) return null
  const max = Math.max(...datos.map(d => d.cantidad), 1)
  const ancho = W / datos.length
  const total = datos.reduce((s, d) => s + d.cantidad, 0)
  return (
    <section className="dsec" aria-label={TEXTOS_DETALLE.reportesPorDia}>
      <h4>{TEXTOS_DETALLE.reportesPorDia} · últimos {datos.length} días</h4>
      <figure role="img" aria-label={`${TEXTOS_DETALLE.reportesPorDia}: ${total} total`} style={{ margin: 0 }}>
        <svg viewBox={`0 0 ${W} ${H}`} preserveAspectRatio="none" style={{ width: '100%', height: H }} aria-hidden="true">
          {datos.map((d, i) => {
            const alto = (d.cantidad / max) * (H - 2)
            return <rect key={d.dia} x={i * ancho + 2} y={H - alto} width={ancho - 4} height={alto} fill="var(--killa)" opacity=".8"><title>{`${d.dia}: ${d.cantidad}`}</title></rect>
          })}
        </svg>
        <TablaOculta caption={TEXTOS_DETALLE.reportesPorDia}>
          <thead><tr><th>Día</th><th>Cantidad</th></tr></thead>
          <tbody>{datos.map(d => <tr key={d.dia}><td>{d.dia}</td><td>{d.cantidad}</td></tr>)}</tbody>
        </TablaOculta>
      </figure>
    </section>
  )
}
