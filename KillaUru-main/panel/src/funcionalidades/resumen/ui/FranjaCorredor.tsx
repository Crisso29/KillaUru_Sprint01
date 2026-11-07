// T076 — FranjaCorredor: barras por tramo + tabla oculta (FR-019, FR-043)
import type { AnomaliaFeature } from '@/api/generado/002-mapa'
import { agruparEnFranja } from '../modelo/franja'
import { TEXTOS_RESUMEN } from '../textos'

const COLORES_SEVERIDAD: Record<number, string> = {
  1: '#22c55e',
  2: '#84cc16',
  3: '#eab308',
  4: '#f97316',
  5: '#ef4444',
}

interface Props {
  anomalias: AnomaliaFeature[]
}

export function FranjaCorredor({ anomalias }: Props) {
  const franjas = agruparEnFranja(anomalias)
  const maxCantidad = Math.max(...franjas.map((f) => f.cantidad), 1)

  if (franjas.length === 0) {
    return <p>{TEXTOS_RESUMEN.vacio.franja}</p>
  }

  return (
    <section aria-label={TEXTOS_RESUMEN.franjaCorredor}>
      <h3>{TEXTOS_RESUMEN.franjaCorredor}</h3>
      <div aria-hidden="true" role="presentation" style={{ display: 'flex', alignItems: 'flex-end', gap: 1 }}>
        {franjas.map((f) => (
          <div
            key={f.tramo}
            title={`km ${f.desdeKm}–${f.desdeKm + 10}: ${f.cantidad} activa(s), sev. ${f.severidadMax}`}
            style={{
              width: 8,
              height: `${(f.cantidad / maxCantidad) * 48}px`,
              background: COLORES_SEVERIDAD[f.severidadMax] ?? '#94a3b8',
            }}
          />
        ))}
      </div>
      <table>
        <caption>{TEXTOS_RESUMEN.franjaCorredor}</caption>
        <thead>
          <tr>
            <th scope="col">Desde km</th>
            <th scope="col">Activas</th>
            <th scope="col">Severidad máx.</th>
          </tr>
        </thead>
        <tbody>
          {franjas.map((f) => (
            <tr key={f.tramo}>
              <td>{f.desdeKm}</td>
              <td>{f.cantidad}</td>
              <td>{f.severidadMax}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  )
}
