// T057 — DialogoRevision: confirmar/retirar cada admin; body {confirmados,retirados}
import { useState } from 'react'
import type { EstadoRevision } from '@/api/generado/001-identidad'
import { useRegistrarRevision } from '../api/useRevision'
import type { DecisionRevision } from '../modelo/revision'
import { armarRevision } from '../modelo/revision'
import { TEXTOS_USUARIOS as T } from '../textos'

interface Props {
  revision: EstadoRevision
  onCerrar: () => void
}

export function DialogoRevision({ revision, onCerrar }: Props) {
  const [decisiones, setDecisiones] = useState<Record<string, DecisionRevision>>({})
  const [error, setError] = useState('')
  const mutation = useRegistrarRevision()

  function setDecision(id: string, decision: DecisionRevision) {
    setDecisiones(prev => ({ ...prev, [id]: decision }))
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    mutation.mutate(armarRevision(decisiones), {
      onSuccess: onCerrar,
      onError: (err) => {
        if (err.codigo === 'revision_incompleta') setError(T.errorRevisionIncompleta)
        else setError('Error inesperado.')
      },
    })
  }

  return (
    <div role="dialog" aria-modal="true" aria-label={T.dialogoRevisionTitulo}>
      <h2>{T.dialogoRevisionTitulo}</h2>
      <form onSubmit={handleSubmit}>
        <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
          {revision.administradores.map(admin => (
            <li key={admin.id} style={{ padding: 'var(--s-8) 0', display: 'flex', alignItems: 'center', gap: 'var(--s-13)' }}>
              <span style={{ flex: 1 }}>{admin.correo}</span>
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-5)' }}>
                <input
                  type="radio"
                  name={`decision-${admin.id}`}
                  value="confirmar"
                  aria-label={T.decisionConfirmar}
                  checked={decisiones[admin.id] === 'confirmar'}
                  onChange={() => setDecision(admin.id, 'confirmar')}
                />
                {T.decisionConfirmar}
              </label>
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--s-5)' }}>
                <input
                  type="radio"
                  name={`decision-${admin.id}`}
                  value="retirar"
                  aria-label={T.decisionRetirar}
                  checked={decisiones[admin.id] === 'retirar'}
                  onChange={() => setDecision(admin.id, 'retirar')}
                />
                {T.decisionRetirar}
              </label>
            </li>
          ))}
        </ul>
        {error && <p role="alert" style={{ color: 'var(--error)' }}>{error}</p>}
        <div style={{ display: 'flex', gap: 'var(--s-8)', justifyContent: 'flex-end', marginTop: 'var(--s-21)' }}>
          <button type="button" onClick={onCerrar}>{T.cancelar}</button>
          <button type="submit" disabled={mutation.isPending}>{T.guardar}</button>
        </div>
      </form>
    </div>
  )
}
