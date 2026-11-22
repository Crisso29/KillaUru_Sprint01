// T076 — FranjaCorredor: barras cada 10 km, color = severidad más alta del tramo, y tabla para lectores (FR-019, FR-043)
import type { AnomaliaFeature } from '@/api/dtos/002-mapa'
import { agruparEnFranja } from '../modelo/franja'
import { TEXTOS_RESUMEN } from '../textos'
import { LARGO_CORREDOR_KM } from '@/config/constantes'

interface Props {
  anomalias: AnomaliaFeature[]
}

// Pueblos del corredor como en el prototipo (Pisco 0 … Ayacucho 333).
const PUEBLOS: Array<[number, string]> = [[0, 'Pisco'], [140, 'Huaytará'], [196, 'Rumichaca'], [268, 'Vinchos'], [333, 'Ayacucho']]
const W = 1000, BASE = 60, ALTO = 48

const x = (km: number) => (km / LARGO_CORREDOR_KM) * (W - 20) + 10

export function FranjaCorredor({ anomalias }: Props) {
  const franjas = agruparEnFranja(anomalias)
  const maximo = Math.max(...franjas.map((f) => f.cantidad), 1)

  return (
    <section className="card strip" aria-label={TEXTOS_RESUMEN.franjaCorredor}>
      <div className="card-h">
        <h3>{TEXTOS_RESUMEN.franjaCorredor}</h3>
        <small>barras cada 10 km · color = severidad más alta del tramo</small>
      </div>
      <div className="card-b">
        {franjas.length === 0 ? (
          <p className="hint">{TEXTOS_RESUMEN.vacio.franja}</p>
        ) : (
          <svg viewBox={`0 0 ${W} 89`} aria-hidden="true" focusable="false">
            {franjas.map((f) => {
              const alto = (f.cantidad / maximo) * ALTO
              return (
                <rect key={f.tramo} x={x(f.desdeKm)} y={BASE - alto} width={(10 / LARGO_CORREDOR_KM) * (W - 20) - 3}
                  height={alto} rx="1.5" fill={`var(--s${f.severidadMax})`}>
                  <title>{`km ${f.desdeKm}–${f.desdeKm + 10}: ${f.cantidad} activa(s), severidad ${f.severidadMax}`}</title>
                </rect>
              )
            })}
            <line x1={10} x2={W - 10} y1={BASE} y2={BASE} stroke="var(--ink-3)" />
            {PUEBLOS.map(([km, nombre]) => (
              <g key={km}>
                <line x1={x(km)} x2={x(km)} y1={BASE} y2={BASE + 6} stroke="var(--ink-3)" />
                <text className="town" x={x(km)} y={80} textAnchor={km === 0 ? 'start' : km === 333 ? 'end' : 'middle'}>
                  {nombre} · {km}
                </text>
              </g>
            ))}
          </svg>
        )}
        <table className="sr-only">
          <caption>{TEXTOS_RESUMEN.franjaCorredor}</caption>
          <thead><tr><th scope="col">Desde km</th><th scope="col">Activas</th><th scope="col">Severidad máx.</th></tr></thead>
          <tbody>
            {franjas.map((f) => (
              <tr key={f.tramo}><td>{f.desdeKm}</td><td>{f.cantidad}</td><td>{f.severidadMax}</td></tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
