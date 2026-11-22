// T047 — DatosAnomalia: ubicación y campos del detalle (FR-025; prototipo `.dsec` + `.kv`)
import type { DetalleAnomalia } from '@/api/dtos/002-mapa'
import { formatearUtcCorto } from '@/compartido/formato/fechas'
import { Hito } from '@/compartido/ui'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { relojReal } from '@/compartido/reloj'
import { dentroDelTramo } from '../modelo/tramoDemo'
import { TEXTOS_DETALLE, TEXTOS_MAPA } from '../textos'

const coord = (v: number, pos: string, neg: string) => `${Math.abs(v).toFixed(4)}° ${v < 0 ? neg : pos}`

function Dato({ etiqueta, children }: { etiqueta: string; children: React.ReactNode }) {
  return <div><span>{etiqueta}</span><b>{children}</b></div>
}

interface Props { detalle: DetalleAnomalia }

export function DatosAnomalia({ detalle }: Props) {
  const nulo = TEXTOS_DETALLE.nulo
  return (
    <>
      <section className="dsec" aria-label="Ubicación">
        <div className="row">
          <Hito km={detalle.kmVia} />
          <div style={{ textAlign: 'right' }}>
            <span className="mono">{coord(detalle.latitud, 'N', 'S')}, {coord(detalle.longitud, 'E', 'O')}</span>
            <p className="hint">{dentroDelTramo(detalle.kmVia) ? `Dentro del ${TEXTOS_MAPA.dentroDemo.toLowerCase()}` : TEXTOS_MAPA.fueraDemo}</p>
          </div>
        </div>
      </section>
      <section className="dsec" aria-label="Datos">
        <div className="kv">
          <Dato etiqueta="Clasificación">{detalle.persistencia}</Dato>
          <Dato etiqueta={TEXTOS_DETALLE.diasDistintos}>{detalle.diasDistintos15d}</Dato>
          <Dato etiqueta={TEXTOS_DETALLE.reportes}>{detalle.reportes}</Dato>
          <Dato etiqueta="Último reporte">{formatearRelativo(detalle.ultimaDeteccionUtc, relojReal.ahora())}</Dato>
          <Dato etiqueta={TEXTOS_DETALLE.vence}>{detalle.venceUtc ? formatearUtcCorto(detalle.venceUtc) : nulo}</Dato>
          <Dato etiqueta={TEXTOS_DETALLE.visibleConductores}>
            <span style={{ color: detalle.visibleConductores ? 'var(--ok)' : 'var(--ink-3)' }}>{detalle.visibleConductores ? 'Sí' : 'No'}</span>
          </Dato>
          <Dato etiqueta={TEXTOS_DETALLE.porRevisar}>{detalle.porRevisar ? 'Sí' : 'No'}</Dato>
          <Dato etiqueta={TEXTOS_DETALLE.vehiculosRevision}>{detalle.vehiculosRevision}</Dato>
        </div>
      </section>
    </>
  )
}
