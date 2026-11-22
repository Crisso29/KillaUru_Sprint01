// T075 — GraficoEventos: barras del periodo, línea punteada del anterior y tabla para lectores (FR-016, FR-043)
import type { EventoPorDia } from '@/api/dtos/004-dashboard'
import { TEXTOS_RESUMEN } from '../textos'

interface Props {
  datos: EventoPorDia[]
  dias: 7 | 14 | 30
  onCambioDias: (dias: 7 | 14 | 30) => void
}

const OPCIONES: Array<7 | 14 | 30> = [7, 14, 30]
// Medidas del prototipo (OV.resumen): lienzo 640 × 233 con márgenes para ejes.
const W = 640, H = 233, IZQ = 34, ABAJO = 21, ARRIBA = 13

/** Escala «redonda» del eje: 350 si el máximo es 321, como en el prototipo. */
const techo = (max: number) => Math.max(50, Math.ceil((max * 1.08) / 50) * 50)

export function GraficoEventos({ datos, dias, onCambioDias }: Props) {
  const max = techo(Math.max(...datos.flatMap((d) => [d.eventos ?? 0, d.eventosPeriodoAnterior ?? 0]), 1))
  const ancho = (W - IZQ) / Math.max(datos.length, 1)
  const y = (v: number) => H - ABAJO - (v / max) * (H - ABAJO - ARRIBA)
  const guias = [0, 1, 2, 3].map((i) => Math.round((max / 3.5) * i / 50) * 50)
  const anterior = datos.map((d, i) => `${i ? 'L' : 'M'}${(IZQ + i * ancho + ancho / 2).toFixed(1)} ${y(d.eventosPeriodoAnterior ?? 0).toFixed(1)}`).join(' ')
  const paso = Math.ceil(datos.length / 8)

  return (
    <section className="card chart" aria-label={TEXTOS_RESUMEN.grafico.titulo}>
      <div className="card-h">
        <h3>{TEXTOS_RESUMEN.grafico.titulo}</h3>
        <fieldset className="seg" style={{ marginLeft: 'auto' }}>
          <legend className="sr-only">Período</legend>
          {OPCIONES.map((op) => (
            <label key={op} className="seg-opcion">
              <input type="radio" name="dias-grafico" value={op} checked={dias === op}
                onChange={() => onCambioDias(op)} aria-label={`${op} días`} className="sr-only" />
              <span aria-hidden="true">{TEXTOS_RESUMEN.selector[`dias${op}` as keyof typeof TEXTOS_RESUMEN.selector]}</span>
            </label>
          ))}
        </fieldset>
      </div>
      <div className="card-b">
        <svg viewBox={`0 0 ${W} ${H}`} aria-hidden="true" focusable="false">
          {guias.map((v) => (
            <g key={v}>
              <line x1={IZQ} x2={W} y1={y(v)} y2={y(v)} stroke="var(--line)" />
              <text x={IZQ - 6} y={y(v) + 3.5} textAnchor="end">{v}</text>
            </g>
          ))}
          {datos.map((d, i) => (
            <rect key={d.dia} x={IZQ + i * ancho + ancho * 0.19} y={y(d.eventos ?? 0)} width={ancho * 0.62}
              height={H - ABAJO - y(d.eventos ?? 0)} rx="2" fill="var(--killa)" opacity={i === datos.length - 1 ? 1 : 0.72}>
              <title>{`${d.dia}: ${d.eventos ?? '—'} eventos (periodo anterior: ${d.eventosPeriodoAnterior ?? '—'})`}</title>
            </rect>
          ))}
          <path d={anterior} fill="none" stroke="var(--ink-3)" strokeWidth="1.5" strokeDasharray="4 3" />
          {datos.map((d, i) => (i % paso === 0 || i === datos.length - 1) && (
            <text key={d.dia} x={IZQ + i * ancho + ancho / 2} y={H - 5} textAnchor="middle">{Number(d.dia.slice(8))}</text>
          ))}
        </svg>
        <table className="sr-only">
          <caption>{TEXTOS_RESUMEN.grafico.titulo}</caption>
          <thead>
            <tr>
              <th scope="col">Día</th>
              <th scope="col">{TEXTOS_RESUMEN.grafico.periodoActual}</th>
              <th scope="col">{TEXTOS_RESUMEN.grafico.periodoAnterior}</th>
            </tr>
          </thead>
          <tbody>
            {datos.map((d) => (
              <tr key={d.dia}><td>{d.dia}</td><td>{d.eventos ?? '—'}</td><td>{d.eventosPeriodoAnterior ?? '—'}</td></tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
