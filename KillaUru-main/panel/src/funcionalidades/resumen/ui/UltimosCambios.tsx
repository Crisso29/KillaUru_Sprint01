// T076 — UltimosCambios: transiciones recientes con «Qué pasó» y «Quién» (FR-020; prototipo tabla `.dt`)
import { Link } from 'react-router'
import type { TransicionReciente } from '@/api/dtos/003-anomalias'
import { textoQuePaso } from '@/compartido/textos/transicion'
import { TEXTOS_RESUMEN } from '../textos'
import { formatearHoraLima } from '@/compartido/formato/fechas'
import { formatearKm } from '@/compartido/formato/km'
import { Icono, InsigniaEstado } from '@/compartido/ui'
import { useActores } from '@/funcionalidades/usuarios'

interface Props {
  transiciones: TransicionReciente[]
}

const COLUMNAS = ['Hora', 'Anomalía', 'Qué pasó', 'Quién', 'Estado']

export function UltimosCambios({ transiciones }: Props) {
  return (
    <section className="card" aria-label={TEXTOS_RESUMEN.ultimosCambios.titulo}>
      <div className="card-h">
        <h3>{TEXTOS_RESUMEN.ultimosCambios.titulo}</h3>
        <Link to="/auditoria" className="lnk">Ver auditoría <Icono nombre="chev" tamano={12} /></Link>
      </div>
      {transiciones.length === 0
        ? <p className="card-b hint">{TEXTOS_RESUMEN.vacio.ultimosCambios}</p>
        : <TablaCambios transiciones={transiciones} />}
    </section>
  )
}

/** «Quién» se ve como en el Historial y la Auditoría: el correo del administrador, o «sistema». */
function TablaCambios({ transiciones }: Props) {
  const correos = useActores(transiciones.map((transicion) => transicion.actorId))
  const quien = (actorId: string | null) => (actorId ? correos[actorId] ?? actorId : TEXTOS_RESUMEN.ultimosCambios.sistema)
  return (
    <table className="dt" style={{ marginTop: 'var(--s-8)' }}>
      <thead><tr>{COLUMNAS.map((columna) => <th key={columna} scope="col">{columna}</th>)}</tr></thead>
      <tbody>
        {transiciones.map((transicion) => (
          <tr key={`${transicion.anomaliaId}-${transicion.ocurridaUtc}`}>
            <td className="mono">{formatearHoraLima(transicion.ocurridaUtc)}</td>
            <td><Link to={`/mapa?anomalia=${transicion.anomaliaId}`}>{transicion.tipo} · km {formatearKm(transicion.kmVia)}</Link></td>
            <td>{textoQuePaso(transicion.desde, transicion.hacia, transicion.actorId)}</td>
            <td>{quien(transicion.actorId)}</td>
            <td aria-label="Estado nuevo"><InsigniaEstado estado={transicion.hacia} /></td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
