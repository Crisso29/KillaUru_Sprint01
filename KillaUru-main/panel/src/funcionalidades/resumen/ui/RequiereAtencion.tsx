// T076 — RequiereAtencion: 4 filas (FR-018, data-model §5)
import { Link } from 'react-router'
import type { ElementoPorRevisar, ElementoPendiente } from '@/api/generado/003-anomalias'
import type { EstadoRevision } from '@/api/generado/001-identidad'
import { contarTemas, primerPendiente } from '../modelo/atencion'
import { TEXTOS_RESUMEN } from '../textos'
import { entorno } from '@/config/entorno'

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
    <section aria-label="Requiere atención">
      <h2>Requiere atención <span aria-label={TEXTOS_RESUMEN.atencion.temas(temas)}>{temas}</span></h2>
      <ul>
        <li>
          <Link to="/moderacion?pestana=por-revisar">
            <span>{TEXTOS_RESUMEN.atencion.porRevisar}</span>
            <span aria-label="Descripción">{TEXTOS_RESUMEN.atencion.porRevisarDetalle}</span>
          </Link>
          <span>{porRevisar.length}</span>
        </li>
        <li>
          <Link to="/moderacion?pestana=pendientes">
            <span>{TEXTOS_RESUMEN.atencion.pendientes}</span>
            {primero && <span aria-label="Primero">{primero}</span>}
          </Link>
          <span>{pendientes.length}</span>
        </li>
        {revision && (
          <li>
            <Link to="/usuarios">
              <span>{TEXTOS_RESUMEN.atencion.revision}</span>
            </Link>
          </li>
        )}
        <li>
          <a
            href={entorno.urlEstado}
            target="_blank"
            rel="noopener noreferrer"
            aria-label={`${TEXTOS_RESUMEN.atencion.estado} (abre en otra pestaña)`}
          >
            <span>{TEXTOS_RESUMEN.atencion.estado}</span>
            <span>{TEXTOS_RESUMEN.atencion.estadoDetalle}</span>
          </a>
        </li>
      </ul>
    </section>
  )
}
