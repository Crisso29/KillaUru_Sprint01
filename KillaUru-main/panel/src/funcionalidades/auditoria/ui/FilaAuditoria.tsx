import { useState } from 'react'
import { Icono } from '@/compartido/ui'
import { TEXTOS_AUDITORIA } from '../textos'
import { textoDeAccion, textoDeCategoria } from '../modelo/textoAccion'
import type { RegistroAuditoria } from '@/api/dtos/002-auditoria'

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

  // Prototipo: fecha en UTC con fuente de datos, «sistema» atenuado, la acción en negrita y la fila se abre con «›».
  return (
    <>
      <tr>
        <td className="mono">{new Date(registro.ocurridoUtc).toISOString().replace('T', ' ').substring(0, 19)}</td>
        <td style={registro.actorId === null ? { color: 'var(--ink-3)' } : undefined}>{actorMostrado}</td>
        <td>{textoDeCategoria(registro.categoria)}</td>
        <td><b>{textoDeAccion(registro.accion, registro.contexto)}</b></td>
        <td className="mono">{registro.objetivo ?? '—'}</td>
        <td className="r">
          <button type="button" className="ob ic" style={{ border: 0, background: 'transparent' }}
            aria-expanded={expandido} aria-label={expandido ? T.contraer : T.expandir} onClick={() => setExpandido(e => !e)}>
            <Icono nombre={expandido ? 'chevd' : 'chev'} tamano={14} />
          </button>
        </td>
      </tr>
      {expandido && (
        <tr className="audx">
          <td colSpan={6}>
            <pre>{JSON.stringify(registro.contexto, null, 2)}</pre>
          </td>
        </tr>
      )}
    </>
  )
}
