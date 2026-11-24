// T076 — FranjaCorredor: barras cada 10 km, color = severidad más alta del tramo, y tabla para lectores (FR-019, FR-043)
import type { AnomaliaFeature } from '@/api/dtos/002-mapa'
import { agruparEnFranja, type TramoFranja as Franja } from '../modelo/franja'
import { TEXTOS_RESUMEN } from '../textos'
import { LARGO_CORREDOR_KM } from '@/config/constantes'

interface Props {
  anomalias: AnomaliaFeature[]
}

// Pueblos del corredor como en el prototipo (Pisco 0 … Ayacucho 333; la vía sigue 2 km más hasta su fin).
const KM_AYACUCHO = 333
const PUEBLOS: Array<[number, string]> = [[0, 'Pisco'], [140, 'Huaytará'], [196, 'Rumichaca'], [268, 'Vinchos'], [KM_AYACUCHO, 'Ayacucho']]
const ANCHO = 1000, BASE = 60, ALTO = 48

const x = (km: number) => (km / LARGO_CORREDOR_KM) * (ANCHO - 20) + 10
/** Los extremos se alinean hacia adentro para que «Pisco» y «Ayacucho» no se corten. */
const anclaPueblo = (km: number) => (km === 0 ? 'start' : km === KM_AYACUCHO ? 'end' : 'middle')

export function FranjaCorredor({ anomalias }: Props) {
  const franjas = agruparEnFranja(anomalias)
  return (
    <section className="card strip" aria-label={TEXTOS_RESUMEN.franjaCorredor}>
      <div className="card-h">
        <h3>{TEXTOS_RESUMEN.franjaCorredor}</h3>
        <small>barras cada 10 km · color = severidad más alta del tramo</small>
      </div>
      <div className="card-b">
        {franjas.length === 0 ? <p className="hint">{TEXTOS_RESUMEN.vacio.franja}</p> : (
          <svg viewBox={`0 0 ${ANCHO} 89`} aria-hidden="true" focusable="false">
            <BarrasCorredor franjas={franjas} />
            <EjePueblos />
          </svg>
        )}
        <TablaFranjas franjas={franjas} />
      </div>
    </section>
  )
}

function BarrasCorredor({ franjas }: { franjas: Franja[] }) {
  const maximo = Math.max(...franjas.map((franja) => franja.cantidad), 1)
  const anchoBarra = (10 / LARGO_CORREDOR_KM) * (ANCHO - 20) - 3
  return franjas.map((franja) => {
    const alto = (franja.cantidad / maximo) * ALTO
    return (
      <rect key={franja.tramo} x={x(franja.desdeKm)} y={BASE - alto} width={anchoBarra} height={alto} rx="1.5" fill={`var(--s${franja.severidadMax})`}>
        <title>{`km ${franja.desdeKm}–${franja.desdeKm + 10}: ${franja.cantidad} activa(s), severidad ${franja.severidadMax}`}</title>
      </rect>
    )
  })
}

function EjePueblos() {
  return (
    <>
      <line x1={10} x2={ANCHO - 10} y1={BASE} y2={BASE} stroke="var(--ink-3)" />
      {PUEBLOS.map(([km, nombre]) => (
        <g key={km}>
          <line x1={x(km)} x2={x(km)} y1={BASE} y2={BASE + 6} stroke="var(--ink-3)" />
          <text className="town" x={x(km)} y={80} textAnchor={anclaPueblo(km)}>{nombre} · {km}</text>
        </g>
      ))}
    </>
  )
}

function TablaFranjas({ franjas }: { franjas: Franja[] }) {
  return (
    <table className="sr-only">
      <caption>{TEXTOS_RESUMEN.franjaCorredor}</caption>
      <thead><tr><th scope="col">Desde km</th><th scope="col">Activas</th><th scope="col">Severidad máx.</th></tr></thead>
      <tbody>
        {franjas.map((franja) => <tr key={franja.tramo}><td>{franja.desdeKm}</td><td>{franja.cantidad}</td><td>{franja.severidadMax}</td></tr>)}
      </tbody>
    </table>
  )
}
