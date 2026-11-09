// T047 — ReportesPorDia: gráfico de barras con tabla oculta accesible (FR-025)
import { TablaOculta } from '@/compartido/ui'
import type { ReportePorDia } from '@/api/dtos/002-mapa'
import { TEXTOS_DETALLE } from '../textos'

interface Props { datos: ReportePorDia[] }

export function ReportesPorDia({ datos }: Props) {
  if (!datos.length) return null
  const max = Math.max(...datos.map(d => d.cantidad), 1)

  return (
    <figure role="img" aria-label={`${TEXTOS_DETALLE.reportesPorDia}: ${datos.reduce((s, d) => s + d.cantidad, 0)} total`}>
      <div style={{ display: 'flex', alignItems: 'flex-end', gap: 2, height: 48 }}>
        {datos.map(d => (
          <div
            key={d.dia}
            title={`${d.dia}: ${d.cantidad}`}
            style={{
              flex: 1,
              height: `${Math.round((d.cantidad / max) * 100)}%`,
              background: 'var(--accent)',
              minHeight: d.cantidad > 0 ? 2 : 0,
            }}
          />
        ))}
      </div>
      <TablaOculta caption={TEXTOS_DETALLE.reportesPorDia}>
        <thead><tr><th>Día</th><th>Cantidad</th></tr></thead>
        <tbody>{datos.map(d => <tr key={d.dia}><td>{d.dia}</td><td>{d.cantidad}</td></tr>)}</tbody>
      </TablaOculta>
    </figure>
  )
}
