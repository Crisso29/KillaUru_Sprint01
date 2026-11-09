// T057 — DialogoDesactivar: requiere motivo ≤200; maneja 409 accion_sobre_si_mismo
import { useState } from 'react'
import { useDesactivarUsuario } from '../api/useAccionesUsuario'
import { TEXTOS_USUARIOS as T } from '../textos'

interface Props {
  usuarioId: string
  onCerrar: () => void
}

export function DialogoDesactivar({ usuarioId, onCerrar }: Props) {
  const [motivo, setMotivo] = useState('')
  const [errorLocal, setErrorLocal] = useState('')
  const mutation = useDesactivarUsuario(usuarioId)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setErrorLocal('')
    if (!motivo.trim()) { setErrorLocal(T.motivoRequerido); return }
    if (motivo.length > 200) { setErrorLocal(T.motivoMaximo); return }
    mutation.mutate({ motivo }, {
      onSuccess: onCerrar,
      onError: (err) => {
        if (err.codigo === 'accion_sobre_si_mismo') setErrorLocal(T.errorAccionSobreSiMismo)
        else if (err.codigo === 'ultimo_administrador') setErrorLocal(T.errorUltimoAdministrador)
        else setErrorLocal('Error inesperado.')
      },
    })
  }

  return (
    <div role="dialog" aria-modal="true" aria-label={T.dialogoDesactivarTitulo}>
      <h2>{T.dialogoDesactivarTitulo}</h2>
      <form onSubmit={handleSubmit}>
        <label>
          <span>{T.motivoLabel}</span>
          <textarea
            aria-label={T.motivoLabel}
            value={motivo}
            onChange={e => setMotivo(e.target.value)}
            maxLength={200}
            rows={3}
            style={{ width: '100%', resize: 'vertical' }}
          />
        </label>
        {(errorLocal || mutation.error) && (
          <p role="alert" style={{ color: 'var(--error)' }}>
            {errorLocal || T.errorAccionSobreSiMismo}
          </p>
        )}
        <div style={{ display: 'flex', gap: 'var(--s-8)', justifyContent: 'flex-end', marginTop: 'var(--s-21)' }}>
          <button type="button" onClick={onCerrar}>{T.cancelar}</button>
          <button type="submit" disabled={mutation.isPending}>{T.confirmar}</button>
        </div>
      </form>
    </div>
  )
}
