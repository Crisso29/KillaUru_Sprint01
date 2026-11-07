// T057 — DialogoInvitar: invita nuevo usuario; maneja 409 correo_en_uso
import { useState } from 'react'
import type { Rol } from '@/api/generado/001-identidad'
import { useCrearUsuario } from '../api/useAccionesUsuario'
import { TEXTOS_USUARIOS as T } from '../textos'

interface Props { onCerrar: () => void }

export function DialogoInvitar({ onCerrar }: Props) {
  const [correo, setCorreo] = useState('')
  const [rol, setRol] = useState<Rol>('ConductorRegistrado')
  const [error, setError] = useState('')
  const mutation = useCrearUsuario()

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    if (!correo.trim()) { setError('El correo es obligatorio.'); return }
    mutation.mutate({ correo: correo.trim(), rol }, {
      onSuccess: onCerrar,
      onError: (err) => {
        if (err.codigo === 'correo_en_uso') setError(T.errorCorreoEnUso)
        else setError('Error inesperado.')
      },
    })
  }

  return (
    <div role="dialog" aria-modal="true" aria-label={T.dialogoInvitarTitulo}>
      <h2>{T.dialogoInvitarTitulo}</h2>
      <form onSubmit={handleSubmit}>
        <label>
          <span>{T.correoLabel}</span>
          <input
            type="email"
            aria-label={T.correoLabel}
            value={correo}
            onChange={e => setCorreo(e.target.value)}
            autoComplete="off"
          />
        </label>
        <label>
          <span>{T.rolLabel}</span>
          <select
            aria-label={T.rolLabel}
            value={rol}
            onChange={e => setRol(e.target.value as Rol)}
          >
            <option value="ConductorRegistrado">Conductor registrado</option>
            <option value="Administrador">Administrador</option>
          </select>
        </label>
        {error && <p role="alert" style={{ color: 'var(--error)' }}>{error}</p>}
        <div style={{ display: 'flex', gap: 'var(--s-8)', justifyContent: 'flex-end', marginTop: 'var(--s-21)' }}>
          <button type="button" onClick={onCerrar}>{T.cancelar}</button>
          <button type="submit" disabled={mutation.isPending}>{T.invitar}</button>
        </div>
      </form>
    </div>
  )
}
