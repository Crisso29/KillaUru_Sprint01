// T047 — DatosAnomalia: filas de los 15 campos escalares (FR-025)
import { formatearKm } from '@/compartido/formato/km'
import type { DetalleAnomalia } from '@/api/dtos/002-mapa'
import { dentroDelTramo } from '../modelo/tramoDemo'
import { TEXTOS_DETALLE, TEXTOS_MAPA } from '../textos'

function Fila({ etiqueta, valor }: { etiqueta: string; valor: React.ReactNode }) {
  return (
    <tr>
      <th scope="row" style={{ color: 'var(--ink-3)', fontWeight: 500, textAlign: 'left', padding: 'var(--s-5)' }}>{etiqueta}</th>
      <td style={{ padding: 'var(--s-5)' }}>{valor}</td>
    </tr>
  )
}

interface Props { detalle: DetalleAnomalia }

export function DatosAnomalia({ detalle }: Props) {
  const nulo = TEXTOS_DETALLE.nulo
  const dentro = dentroDelTramo(detalle.kmVia)
  return (
    <table style={{ borderCollapse: 'collapse', width: '100%', fontSize: 'var(--t-14)' }}>
      <tbody>
        <Fila etiqueta={TEXTOS_DETALLE.id} valor={<code>{detalle.id}</code>} />
        <Fila etiqueta={TEXTOS_DETALLE.tipo} valor={detalle.tipo} />
        <Fila etiqueta={TEXTOS_DETALLE.severidad} valor={detalle.severidadMostrada} />
        <Fila etiqueta={TEXTOS_DETALLE.estado} valor={detalle.estado} />
        <Fila etiqueta={TEXTOS_DETALLE.persistencia} valor={detalle.persistencia} />
        <Fila etiqueta={TEXTOS_DETALLE.coordenadas} valor={`${detalle.latitud}, ${detalle.longitud}`} />
        <Fila etiqueta={TEXTOS_DETALLE.km} valor={
          <>km {formatearKm(detalle.kmVia)} · <span>{dentro ? TEXTOS_MAPA.dentroDemo : TEXTOS_MAPA.fueraDemo}</span></>
        } />
        <Fila etiqueta={TEXTOS_DETALLE.diasDistintos} valor={detalle.diasDistintos15d} />
        <Fila etiqueta={TEXTOS_DETALLE.reportes} valor={detalle.reportes} />
        <Fila etiqueta={TEXTOS_DETALLE.ultimaDeteccion} valor={detalle.ultimaDeteccionUtc} />
        <Fila etiqueta={TEXTOS_DETALLE.vence} valor={detalle.venceUtc ?? nulo} />
        <Fila etiqueta={TEXTOS_DETALLE.visibleConductores} valor={detalle.visibleConductores ? 'Sí' : 'No'} />
        <Fila etiqueta={TEXTOS_DETALLE.porRevisar} valor={detalle.porRevisar ? 'Sí' : 'No'} />
        <Fila etiqueta={TEXTOS_DETALLE.vehiculosRevision} valor={detalle.vehiculosRevision} />
      </tbody>
    </table>
  )
}
