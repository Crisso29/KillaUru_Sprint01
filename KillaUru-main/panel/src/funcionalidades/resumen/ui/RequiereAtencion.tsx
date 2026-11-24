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

const textos = TEXTOS_RESUMEN.atencion

export function RequiereAtencion({ porRevisar, pendientes, revision, ahora = new Date() }: Props) {
  const temas = revision ? contarTemas(porRevisar.length, pendientes.length, revision, ahora) : porRevisar.length + pendientes.length
  const primero = primerPendiente(pendientes, ahora)
  return (
    <section className="card" aria-label="Requiere atención">
      <div className="card-h"><h3>Requiere atención</h3><small aria-label={textos.temas(temas)}>{textos.temas(temas)}</small></div>
      <ul className="att" style={{ marginTop: 'var(--s-8)', listStyle: 'none' }}>
        <FilaAtencion a="/moderacion?pestana=por-revisar" color="pendiente" titulo={textos.porRevisar} detalle={textos.porRevisarDetalle} cantidad={porRevisar.length} />
        <FilaAtencion a="/moderacion?pestana=pendientes" color="pendiente" titulo={textos.pendientes} detalle={primero} cantidad={pendientes.length} etiquetaDetalle="Primero" />
        {revision && <FilaAtencion a="/usuarios" color="reparada" titulo={textos.revision} />}
        <FilaEstadoServicio />
      </ul>
    </section>
  )
}

interface PropsFila { a: string; color: string; titulo: string; detalle?: string | null; cantidad?: number; etiquetaDetalle?: string }

function FilaAtencion({ a, color, titulo, detalle, cantidad, etiquetaDetalle }: PropsFila) {
  return (
    <li>
      <Link to={a} className="fila-att">
        <span className={`sd ${color}`} aria-hidden="true" />
        <span><b>{titulo}</b>{detalle && <small aria-label={etiquetaDetalle}>{detalle}</small>}</span>
        <span className="n">{cantidad}</span>
        <Icono nombre="chev" tamano={14} />
      </Link>
    </li>
  )
}

/** Página de estado externa: se abre en otra pestaña y lo anuncia a los lectores de pantalla. */
function FilaEstadoServicio() {
  return (
    <li>
      <a href={entorno.urlEstado} target="_blank" rel="noopener noreferrer" className="fila-att" aria-label={`${textos.estado} (abre en otra pestaña)`}>
        <span className="sd activa" aria-hidden="true" />
        <span><b>{textos.estado}</b><small>{textos.estadoDetalle}</small></span>
        <span className="n" />
        <Icono nombre="ext" tamano={14} />
      </a>
    </li>
  )
}
