// T075 — GraficoEventos: barras + línea punteada + tabla oculta (FR-016, FR-043)
import type { EventoPorDia } from '@/api/generado/004-dashboard'
import { TEXTOS_RESUMEN } from '../textos'

interface Props {
  datos: EventoPorDia[]
  dias: 7 | 14 | 30
  onCambioDias: (dias: 7 | 14 | 30) => void
}

const OPCIONES: Array<7 | 14 | 30> = [7, 14, 30]

export function GraficoEventos({ datos, dias, onCambioDias }: Props) {
  const max = Math.max(
    ...datos.flatMap((d) => [d.eventos ?? 0, d.eventosPeriodoAnterior ?? 0]),
    1,
  )

  return (
    <section aria-label={TEXTOS_RESUMEN.grafico.titulo}>
      <div>
        <h3>{TEXTOS_RESUMEN.grafico.titulo}</h3>
        <fieldset>
          <legend>Período</legend>
          {OPCIONES.map((op) => (
            <label key={op}>
              <input
                type="radio"
                name="dias-grafico"
                value={op}
                checked={dias === op}
                onChange={() => onCambioDias(op)}
                aria-label={`${op} días`}
              />
              {TEXTOS_RESUMEN.selector[`dias${op}` as keyof typeof TEXTOS_RESUMEN.selector]}
            </label>
          ))}
        </fieldset>
      </div>
      <div aria-hidden="true" role="presentation" style={{ display: 'flex', alignItems: 'flex-end', gap: 2 }}>
        {datos.map((d) => (
          <div key={d.dia} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <div
              style={{ width: '100%', background: 'currentColor', height: `${((d.eventos ?? 0) / max) * 60}px` }}
            />
          </div>
        ))}
      </div>
      <table>
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
            <tr key={d.dia}>
              <td>{d.dia}</td>
              <td>{d.eventos ?? '—'}</td>
              <td>{d.eventosPeriodoAnterior ?? '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  )
}
