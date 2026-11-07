import { useState } from 'react'
import { TEXTOS_AUDITORIA } from '../textos'
import { textoDeAccion, textoDeCategoria } from '../modelo/textoAccion'
import type { RegistroAuditoria } from '@/api/generado/002-auditoria'

interface Props {
  registro: RegistroAuditoria
  correoActor?: string | null
}

export function FilaAuditoria({ registro, correoActor }: Props) {
  const [expandido, setExpandido] = useState(false)
  const T = TEXTOS_AUDITORIA

  const actor = correoActor === undefined
    ? registro.actorId ?? T.sistema
    : correoActor === null
      ? T.sinCuenta
      : correoActor

  const actorMostrado = registro.actorId === null ? T.sistema : actor

  return (
    <>
      <tr>
        <td>{new Date(registro.ocurridoUtc).toISOString().replace('T', ' ').substring(0, 19)}</td>
        <td>{actorMostrado}</td>
        <td>{textoDeCategoria(registro.categoria)}</td>
        <td>{textoDeAccion(registro.accion, registro.contexto)}</td>
        <td>{registro.objetivo ?? '—'}</td>
        <td>
          <button
            aria-expanded={expandido}
            onClick={() => setExpandido(e => !e)}
          >
            {expandido ? T.contraer : T.expandir}
          </button>
        </td>
      </tr>
      {expandido && (
        <tr>
          <td colSpan={6}>
            <pre>{JSON.stringify(registro.contexto, null, 2)}</pre>
          </td>
        </tr>
      )}
    </>
  )
}
