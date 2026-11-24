// T078 — tabla de la auditoría: una fila por registro
import { TEXTOS_AUDITORIA as textos } from '../textos'
import { FilaAuditoria } from './FilaAuditoria'
import type { RegistroAuditoria } from '@/api/dtos/002-auditoria'

interface Props {
  registros: RegistroAuditoria[]
  /** actorId → correo (de `useActores`); un id sin correo se muestra como id. */
  correosActores: Record<string, string>
}

const COLUMNAS = [textos.columnas.hora, textos.columnas.actor, textos.columnas.categoria, textos.columnas.accion, textos.columnas.objetivo]

export function TablaAuditoria({ registros, correosActores }: Props) {
  if (registros.length === 0) return <p className="card-b hint">{textos.vacia}</p>
  return (
    <table className="dt">
      <thead>
        <tr>
          {COLUMNAS.map((columna) => <th key={columna} scope="col">{columna}</th>)}
          <th scope="col"><span className="sr-only">Contexto</span></th>
        </tr>
      </thead>
      <tbody>
        {registros.map((registro) => (
          <FilaAuditoria key={registro.id} registro={registro}
            correoActor={registro.actorId ? correosActores[registro.actorId] ?? null : null} />
        ))}
      </tbody>
    </table>
  )
}
