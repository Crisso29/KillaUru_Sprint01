// T076 — UltimosCambios: transiciones recientes con «Qué pasó» y «Quién» (FR-020; prototipo tabla `.dt`)
import { Link } from 'react-router'
import type { TransicionReciente } from '@/api/dtos/003-anomalias'
import { textoQuePaso } from '../modelo/transicion'
import { TEXTOS_RESUMEN } from '../textos'
import { formatearHoraLima } from '@/compartido/formato/fechas'
import { Icono, InsigniaEstado } from '@/compartido/ui'

interface Props {
  transiciones: TransicionReciente[]
}

const km = (v: number) => v.toLocaleString('es-PE', { minimumFractionDigits: 1, maximumFractionDigits: 1 })

export function UltimosCambios({ transiciones }: Props) {
  return (
    <section className="card" aria-label={TEXTOS_RESUMEN.ultimosCambios.titulo}>
      <div className="card-h">
        <h3>{TEXTOS_RESUMEN.ultimosCambios.titulo}</h3>
        <Link to="/auditoria" className="lnk">Ver auditoría <Icono nombre="chev" tamano={12} /></Link>
      </div>
      {transiciones.length === 0 ? (
        <p className="card-b hint">{TEXTOS_RESUMEN.vacio.ultimosCambios}</p>
      ) : (
        <table className="dt" style={{ marginTop: 'var(--s-8)' }}>
          <thead>
            <tr><th scope="col">Hora</th><th scope="col">Anomalía</th><th scope="col">Qué pasó</th><th scope="col">Quién</th><th scope="col">Estado</th></tr>
          </thead>
          <tbody>
            {transiciones.map((t) => (
              <tr key={`${t.anomaliaId}-${t.ocurridaUtc}`}>
                <td className="mono">{formatearHoraLima(t.ocurridaUtc)}</td>
                <td><Link to={`/mapa?anomalia=${t.anomaliaId}`}>{t.tipo} · km {km(t.kmVia)}</Link></td>
                <td>{textoQuePaso(t.desde, t.hacia, t.actorId)}</td>
                <td>{t.actorId ? t.actorId.slice(0, 8) : TEXTOS_RESUMEN.ultimosCambios.sistema}</td>
                <td aria-label="Estado nuevo"><InsigniaEstado estado={t.hacia} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  )
}
