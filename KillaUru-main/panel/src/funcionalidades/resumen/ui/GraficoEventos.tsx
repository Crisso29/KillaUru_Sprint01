// T075 — GraficoEventos: barras del periodo, línea punteada del anterior y tabla para lectores (FR-016, FR-043)
import type { EventoPorDia } from '@/api/dtos/004-dashboard'
import { TEXTOS_RESUMEN } from '../textos'
import { escalaGrafico, LIENZO } from '../modelo/escalaGrafico'

type Dias = 7 | 14 | 30

interface Props {
  datos: EventoPorDia[]
  dias: Dias
  onCambioDias: (dias: Dias) => void
}

const OPCIONES: Dias[] = [7, 14, 30]
const { ANCHO, ALTO, IZQ, ABAJO } = LIENZO

export function GraficoEventos({ datos, dias, onCambioDias }: Props) {
  return (
    <section className="card chart" aria-label={TEXTOS_RESUMEN.grafico.titulo}>
      <div className="card-h">
        <h3>{TEXTOS_RESUMEN.grafico.titulo}</h3>
        <SelectorDias dias={dias} onCambio={onCambioDias} />
      </div>
      <div className="card-b">
        <Lienzo datos={datos} />
        <TablaEventos datos={datos} />
      </div>
    </section>
  )
}

function SelectorDias({ dias, onCambio }: { dias: Dias; onCambio: (dias: Dias) => void }) {
  return (
    <fieldset className="seg" style={{ marginLeft: 'auto' }}>
      <legend className="sr-only">Período</legend>
      {OPCIONES.map((opcion) => (
        <label key={opcion} className="seg-opcion">
          <input type="radio" name="dias-grafico" value={opcion} checked={dias === opcion}
            onChange={() => onCambio(opcion)} aria-label={`${opcion} días`} className="sr-only" />
          <span aria-hidden="true">{TEXTOS_RESUMEN.selector[`dias${opcion}`]}</span>
        </label>
      ))}
    </fieldset>
  )
}

/** El dibujo es decorativo (`aria-hidden`): la tabla oculta da los mismos datos a los lectores de pantalla. */
function Lienzo({ datos }: { datos: EventoPorDia[] }) {
  const { ancho, y, centro, trazoAnterior, guias, paso } = escalaGrafico(datos)
  const esRotulado = (i: number) => i % paso === 0 || i === datos.length - 1
  return (
    <svg viewBox={`0 0 ${ANCHO} ${ALTO}`} aria-hidden="true" focusable="false">
      <Guias guias={guias} y={y} />
      {datos.map((fila, i) => (
        <rect key={fila.dia} x={IZQ + i * ancho + ancho * 0.19} y={y(fila.eventos ?? 0)} width={ancho * 0.62}
          height={ALTO - ABAJO - y(fila.eventos ?? 0)} rx="2" fill="var(--killa)" opacity={i === datos.length - 1 ? 1 : 0.72}>
          <title>{`${fila.dia}: ${fila.eventos ?? '—'} eventos (periodo anterior: ${fila.eventosPeriodoAnterior ?? '—'})`}</title>
        </rect>
      ))}
      <path d={trazoAnterior} fill="none" stroke="var(--ink-3)" strokeWidth="1.5" strokeDasharray="4 3" />
      {datos.map((fila, i) => esRotulado(i) && (
        <text key={fila.dia} x={centro(i)} y={ALTO - 5} textAnchor="middle">{Number(fila.dia.slice(8))}</text>
      ))}
    </svg>
  )
}

/** Líneas horizontales del eje con su valor a la izquierda. */
function Guias({ guias, y }: { guias: number[]; y: (valor: number) => number }) {
  return guias.map((guia) => (
    <g key={guia}>
      <line x1={IZQ} x2={ANCHO} y1={y(guia)} y2={y(guia)} stroke="var(--line)" />
      <text x={IZQ - 6} y={y(guia) + 3.5} textAnchor="end">{guia}</text>
    </g>
  ))
}

function TablaEventos({ datos }: { datos: EventoPorDia[] }) {
  const { periodoActual, periodoAnterior, titulo } = TEXTOS_RESUMEN.grafico
  return (
    <table className="sr-only">
      <caption>{titulo}</caption>
      <thead><tr><th scope="col">Día</th><th scope="col">{periodoActual}</th><th scope="col">{periodoAnterior}</th></tr></thead>
      <tbody>
        {datos.map((fila) => (
          <tr key={fila.dia}><td>{fila.dia}</td><td>{fila.eventos ?? '—'}</td><td>{fila.eventosPeriodoAnterior ?? '—'}</td></tr>
        ))}
      </tbody>
    </table>
  )
}
