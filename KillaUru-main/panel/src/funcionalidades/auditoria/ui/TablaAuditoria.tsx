import { TEXTOS_AUDITORIA } from '../textos'
import { FilaAuditoria } from './FilaAuditoria'
import type { RegistroAuditoria } from '@/api/generado/002-auditoria'

interface Props {
  registros: RegistroAuditoria[]
  correosActores: Map<string, string | null>
}

export function TablaAuditoria({ registros, correosActores }: Props) {
  const T = TEXTOS_AUDITORIA

  if (registros.length === 0) {
    return <p>{T.vacia}</p>
  }

  return (
    <table>
      <thead>
        <tr>
          <th>{T.columnas.hora}</th>
          <th>{T.columnas.actor}</th>
          <th>{T.columnas.categoria}</th>
          <th>{T.columnas.accion}</th>
          <th>{T.columnas.objetivo}</th>
          <th></th>
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
