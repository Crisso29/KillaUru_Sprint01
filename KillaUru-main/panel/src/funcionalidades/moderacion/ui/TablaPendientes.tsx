// T067 — TablaPendientes: solo lectura, con «Ver en el mapa» (FR-029; prototipo tabla `.dt`)
import { Link } from 'react-router'
import { Severidad } from '@/compartido/ui'
import { formatearKm } from '@/compartido/formato/km'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { formatearUtcCorto } from '@/compartido/formato/fechas'
import { relojReal } from '@/compartido/reloj'
import { TEXTOS_MODERACION as T } from '../textos'
import type { ElementoPendiente } from '@/api/dtos/003-anomalias'

interface Props {
  items: ElementoPendiente[]
}

export function TablaPendientes({ items }: Props) {
  if (items.length === 0) {
    return <div className="card"><p className="card-b hint">{T.vacioPendientes}</p></div>
  }
  const ahora = relojReal.ahora()
  return (
    <div className="card">
      <table className="dt">
        <thead>
          <tr>
            <th scope="col">Anomalía</th>
            <th scope="col">Ubicación</th>
            <th scope="col" className="r">Reportes</th>
            <th scope="col">Primer reporte</th>
            <th scope="col">Vence</th>
            <th scope="col"><span className="sr-only">Acciones</span></th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id}>
              <td>
                <span style={{ display: 'inline-flex', gap: 'var(--s-5)', alignItems: 'center' }}>
                  <Severidad nivel={item.severidadMostrada} tamano="sm" />
                  <b>{item.tipo}</b> <span className="mono">{item.id.slice(0, 8)}</span>
                </span>
              </td>
              <td>km {formatearKm(item.kmVia)}</td>
              <td className="r">{item.reportes}</td>
              <td>{formatearRelativo(item.primerReporteUtc, ahora)}</td>
              <td className="mono">{formatearUtcCorto(item.venceUtc)}</td>
              <td className="r"><Link to={`/mapa?anomalia=${item.id}`} className="lnk">{T.verEnMapa}</Link></td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="tfoot"><span>{items.length} {items.length === 1 ? 'elemento' : 'elementos'}</span><span>RN-08</span></div>
    </div>
  )
}
