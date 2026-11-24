// T057 — DialogoRevision: confirmar/retirar cada admin; body {confirmados,retirados}
import { useState } from 'react'
import type { EstadoRevision } from '@/api/dtos/001-identidad'
import { useRegistrarRevision } from '../api/useRevision'
import { armarRevision, type DecisionRevision } from '../modelo/revision'
import { mensajeDeErrorAccion } from '../modelo/erroresAccion'
import { TEXTOS_USUARIOS as textos } from '../textos'
import { FormularioDialogo } from './FormularioDialogo'

interface Props { revision: EstadoRevision; onCerrar: () => void }

type Administrador = EstadoRevision['administradores'][number]

const DECISIONES: { valor: DecisionRevision; texto: string }[] = [
  { valor: 'confirmar', texto: textos.decisionConfirmar },
  { valor: 'retirar', texto: textos.decisionRetirar },
]

export function DialogoRevision({ revision, onCerrar }: Props) {
  const [decisiones, setDecisiones] = useState<Record<string, DecisionRevision>>({})
  const [error, setError] = useState('')
  const registrar = useRegistrarRevision()
  const decidir = (id: string, decision: DecisionRevision) => setDecisiones((anteriores) => ({ ...anteriores, [id]: decision }))
  const guardar = () => {
    setError('')
    registrar.mutate(armarRevision(decisiones), { onSuccess: onCerrar, onError: (fallo) => setError(mensajeDeErrorAccion(fallo.codigo)) })
  }
  return (
    <FormularioDialogo titulo={textos.dialogoRevisionTitulo} error={error} ocupado={registrar.isPending}
      textoConfirmar={textos.guardar} onEnviar={guardar} onCerrar={onCerrar}>
      <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
        {revision.administradores.map((admin) => (
          <FilaRevision key={admin.id} admin={admin} decision={decisiones[admin.id]} onDecidir={(decision) => decidir(admin.id, decision)} />
        ))}
      </ul>
    </FormularioDialogo>
  )
}

interface PropsFila { admin: Administrador; decision?: DecisionRevision; onDecidir: (decision: DecisionRevision) => void }

function FilaRevision({ admin, decision, onDecidir }: PropsFila) {
  return (
    <li style={{ padding: 'var(--s-8) 0', display: 'flex', alignItems: 'center', gap: 'var(--s-13)' }}>
      <span style={{ flex: 1 }}>{admin.correo}</span>
      {DECISIONES.map((opcion) => (
        <label key={opcion.valor} style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-5)' }}>
          <input type="radio" name={`decision-${admin.id}`} value={opcion.valor} aria-label={opcion.texto}
            checked={decision === opcion.valor} onChange={() => onDecidir(opcion.valor)} />
          {opcion.texto}
        </label>
      ))}
    </li>
  )
}
