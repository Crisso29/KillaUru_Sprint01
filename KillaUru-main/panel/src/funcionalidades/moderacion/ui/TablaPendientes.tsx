// T067 — TablaPendientes: solo lectura, con «Ver en el mapa» (FR-029; prototipo tabla `.dt`)
import { Link } from 'react-router'
import { formatearKm } from '@/compartido/formato/km'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { formatearUtcCorto } from '@/compartido/formato/fechas'
import { relojReal } from '@/compartido/reloj'
import { TEXTOS_MODERACION as textos } from '../textos'
import type { ElementoPendiente } from '@/api/dtos/003-anomalias'
import { Cabecera, CeldaAnomalia, PieTabla, type Columna } from './PartesTabla'

const COLUMNAS: Columna[] = [
  { texto: 'Anomalía' }, { texto: 'Ubicación' }, { texto: 'Reportes', derecha: true }, { texto: 'Primer reporte' },
  { texto: 'Vence' }, { texto: 'Acciones', oculta: true },
]

export function TablaPendientes({ items }: { items: ElementoPendiente[] }) {
  if (items.length === 0) return <div className="card"><p className="card-b hint">{textos.vacioPendientes}</p></div>
  const ahora = relojReal.ahora()
  return (
    <div className="card">
      <table className="dt">
        <Cabecera columnas={COLUMNAS} />
        <tbody>{items.map((item) => <FilaPendiente key={item.id} item={item} ahora={ahora} />)}</tbody>
      </table>
      <PieTabla cantidad={items.length} reglas="RN-08" />
    </div>
  )
}

function FilaPendiente({ item, ahora }: { item: ElementoPendiente; ahora: Date }) {
  return (
    <tr>
      <CeldaAnomalia id={item.id} tipo={item.tipo} severidad={item.severidadMostrada} />
      <td>km {formatearKm(item.kmVia)}</td>
      <td className="r">{item.reportes}</td>
      <td>{formatearRelativo(item.primerReporteUtc, ahora)}</td>
      <td className="mono">{formatearUtcCorto(item.venceUtc)}</td>
      <td className="r"><Link to={`/mapa?anomalia=${item.id}`} className="lnk">{textos.verEnMapa}</Link></td>
    </tr>
  )
}
