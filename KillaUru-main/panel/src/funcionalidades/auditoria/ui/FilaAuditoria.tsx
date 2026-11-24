// T078 — una fila de la auditoría; «›» abre el contexto completo en JSON (prototipo)
import { useState } from 'react'
import { Icono } from '@/compartido/ui'
import { TEXTOS_AUDITORIA as textos } from '../textos'
import { textoDeAccion, textoDeCategoria } from '../modelo/textoAccion'
import { nombreDelActor } from '../modelo/nombreActor'
import type { RegistroAuditoria } from '@/api/dtos/002-auditoria'

interface Props {
  registro: RegistroAuditoria
  correoActor?: string | null
}

/** «2026-11-23 15:00:00»: UTC, como lo guarda el servidor (la nota «Hora en UTC» lo aclara arriba). */
const fechaUtc = (iso: string) => new Date(iso).toISOString().replace('T', ' ').substring(0, 19)

export function FilaAuditoria({ registro, correoActor }: Props) {
  const [expandido, setExpandido] = useState(false)
  return (
    <>
      <tr>
        <td className="mono">{fechaUtc(registro.ocurridoUtc)}</td>
        <td style={registro.actorId === null ? { color: 'var(--ink-3)' } : undefined}>{nombreDelActor(registro.actorId, correoActor)}</td>
        <td>{textoDeCategoria(registro.categoria)}</td>
        <td><b>{textoDeAccion(registro.accion, registro.contexto)}</b></td>
        <td className="mono">{registro.objetivo ?? '—'}</td>
        <td className="r"><BotonExpandir expandido={expandido} onCambiar={() => setExpandido((abierto) => !abierto)} /></td>
      </tr>
      {expandido && <tr className="audx"><td colSpan={6}><pre>{JSON.stringify(registro.contexto, null, 2)}</pre></td></tr>}
    </>
  )
}

function BotonExpandir({ expandido, onCambiar }: { expandido: boolean; onCambiar: () => void }) {
  return (
    <button type="button" className="ob ic" style={{ border: 0, background: 'transparent' }} aria-expanded={expandido}
      aria-label={expandido ? textos.contraer : textos.expandir} onClick={onCambiar}>
      <Icono nombre={expandido ? 'chevd' : 'chev'} tamano={14} />
    </button>
  )
}
