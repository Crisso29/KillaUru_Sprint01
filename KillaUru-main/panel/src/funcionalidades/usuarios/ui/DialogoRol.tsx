// T057 — DialogoRol: cambia rol de un usuario; maneja 409 ultimo_administrador
import { useState } from 'react'
import type { Rol } from '@/api/generado/001-identidad'
import { useCambiarRol } from '../api/useAccionesUsuario'
import { TEXTOS_USUARIOS as T } from '../textos'

interface Props {
  usuarioId: string
  rolActual: Rol
  onCerrar: () => void
}

export function DialogoRol({ usuarioId, rolActual, onCerrar }: Props) {
  const [rol, setRol] = useState<Rol>(rolActual === 'Administrador' ? 'ConductorRegistrado' : 'Administrador')
  const [error, setError] = useState('')
  const mutation = useCambiarRol(usuarioId)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    mutation.mutate({ rol }, {
      onSuccess: onCerrar,
      onError: (err) => {
        if (err.codigo === 'ultimo_administrador') setError(T.errorUltimoAdministrador)
        else if (err.codigo === 'accion_sobre_si_mismo') setError(T.errorAccionSobreSiMismo)
        else setError('Error inesperado.')
      },
    })
  }

  return (
    <div role="dialog" aria-modal="true" aria-label={T.dialogoCambiarRolTitulo}>
      <h2>{T.dialogoCambiarRolTitulo}</h2>
      <form onSubmit={handleSubmit}>
        <label>
          <span>{T.rolLabel}</span>
          <select
            aria-label={T.rolLabel}
            value={rol}
            onChange={e => setRol(e.target.value as Rol)}
          >
            <option value="Administrador">Administrador</option>
            <option value="ConductorRegistrado">Conductor registrado</option>
          </select>
        </label>
        {error && <p role="alert" style={{ color: 'var(--error)' }}>{error}</p>}
        <div style={{ display: 'flex', gap: 'var(--s-8)', justifyContent: 'flex-end', marginTop: 'var(--s-21)' }}>
          <button type="button" onClick={onCerrar}>{T.cancelar}</button>
          <button type="submit" disabled={mutation.isPending}>{T.confirmar}</button>
        </div>
      </form>
    </div>
  )
}
