// T076 — RequiereAtencion: temas que piden acción (FR-018, data-model §5; prototipo `.att`)
import { Link } from 'react-router'
import type { ElementoPorRevisar, ElementoPendiente } from '@/api/dtos/003-anomalias'
import type { EstadoRevision } from '@/api/dtos/001-identidad'
import { contarTemas, primerPendiente } from '../modelo/atencion'
import { TEXTOS_RESUMEN } from '../textos'
import { entorno } from '@/config/entorno'
import { Icono } from '@/compartido/ui'

interface Props {
  porRevisar: ElementoPorRevisar[]
  pendientes: ElementoPendiente[]
  revision: EstadoRevision | undefined
  ahora?: Date
}

export function RequiereAtencion({ porRevisar, pendientes, revision, ahora = new Date() }: Props) {
  const temas = revision
    ? contarTemas(porRevisar.length, pendientes.length, revision, ahora)
    : porRevisar.length + pendientes.length
  const primero = primerPendiente(pendientes, ahora)

  return (
    <section className="card" aria-label="Requiere atención">
      <div className="card-h">
        <h3>Requiere atención</h3>
        <small aria-label={TEXTOS_RESUMEN.atencion.temas(temas)}>{TEXTOS_RESUMEN.atencion.temas(temas)}</small>
      </div>
      <ul className="att" style={{ marginTop: 'var(--s-8)', listStyle: 'none' }}>
        <li>
          <Link to="/moderacion?pestana=por-revisar" className="fila-att">
            <span className="sd pendiente" aria-hidden="true" />
            <span><b>{TEXTOS_RESUMEN.atencion.porRevisar}</b><small>{TEXTOS_RESUMEN.atencion.porRevisarDetalle}</small></span>
            <span className="n">{porRevisar.length}</span>
            <Icono nombre="chev" tamano={14} />
          </Link>
        </li>
        <li>
          <Link to="/moderacion?pestana=pendientes" className="fila-att">
            <span className="sd pendiente" aria-hidden="true" />
            <span><b>{TEXTOS_RESUMEN.atencion.pendientes}</b>{primero && <small aria-label="Primero">{primero}</small>}</span>
            <span className="n">{pendientes.length}</span>
            <Icono nombre="chev" tamano={14} />
          </Link>
        </li>
        {revision && (
          <li>
            <Link to="/usuarios" className="fila-att">
              <span className="sd reparada" aria-hidden="true" />
              <span><b>{TEXTOS_RESUMEN.atencion.revision}</b></span>
              <span className="n" />
              <Icono nombre="chev" tamano={14} />
            </Link>
          </li>
        )}
        <li>
          <a href={entorno.urlEstado} target="_blank" rel="noopener noreferrer" className="fila-att"
            aria-label={`${TEXTOS_RESUMEN.atencion.estado} (abre en otra pestaña)`}>
            <span className="sd activa" aria-hidden="true" />
            <span><b>{TEXTOS_RESUMEN.atencion.estado}</b><small>{TEXTOS_RESUMEN.atencion.estadoDetalle}</small></span>
            <span className="n" />
            <Icono nombre="ext" tamano={14} />
          </a>
        </li>
      </ul>
    </section>
  )
}
