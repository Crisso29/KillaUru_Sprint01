import { TEXTOS_AUDITORIA } from '../textos'
import { FilaAuditoria } from './FilaAuditoria'
import type { RegistroAuditoria } from '@/api/dtos/002-auditoria'

interface Props {
  registros: RegistroAuditoria[]
  correosActores: Map<string, string | null>
}

export function TablaAuditoria({ registros, correosActores }: Props) {
  const T = TEXTOS_AUDITORIA

  if (registros.length === 0) {
    return <p className="card-b hint">{T.vacia}</p>
  }

  return (
    <table className="dt">
      <thead>
        <tr>
          <th scope="col">{T.columnas.hora}</th>
          <th scope="col">{T.columnas.actor}</th>
          <th scope="col">{T.columnas.categoria}</th>
          <th scope="col">{T.columnas.accion}</th>
          <th scope="col">{T.columnas.objetivo}</th>
          <th scope="col"><span className="sr-only">Contexto</span></th>
        </tr>
      </thead>
      <tbody>
        {registros.map(r => (
          <FilaAuditoria
            key={r.id}
            registro={r}
            correoActor={r.actorId ? correosActores.get(r.actorId) : null}
          />
        ))}
      </tbody>
    </table>
  )
}
