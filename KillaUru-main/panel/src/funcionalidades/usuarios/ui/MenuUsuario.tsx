// T056 — MenuUsuario: acciones por estado; fila propia sin Cambiar rol ni Desactivar
import { useState } from 'react'
import type { Usuario } from '@/api/dtos/001-identidad'
import { useActivarUsuario, useReenviarCorreo } from '../api/useAccionesUsuario'
import { DialogoRol } from './DialogoRol'
import { DialogoDesactivar } from './DialogoDesactivar'
import { TEXTOS_USUARIOS as T } from '../textos'

type Dialogo = 'rol' | 'desactivar' | null

interface Props {
  usuario: Usuario
  correoActual: string
}

export function MenuUsuario({ usuario, correoActual }: Props) {
  const [abierto, setAbierto] = useState(false)
  const [dialogo, setDialogo] = useState<Dialogo>(null)
  const esPropiaFila = usuario.correo === correoActual

  const activar = useActivarUsuario(usuario.id)
  const reenviar = useReenviarCorreo(usuario.id)

  function cerrarTodo() {
    setDialogo(null)
    setAbierto(false)
  }

  function accion(fn: () => void) {
    setAbierto(false)
    fn()
  }

  const puedeReenviar =
    !usuario.correoVerificado || usuario.estado === 'InvitacionPendiente'

  return (
    <div style={{ position: 'relative' }}>
      <button
        type="button"
        aria-label={T.accionesLabel}
        aria-haspopup="menu"
        aria-expanded={abierto}
        onClick={() => setAbierto(v => !v)}
        style={{ padding: 'var(--s-5) var(--s-8)', cursor: 'pointer' }}
      >
        ⋮
      </button>

      {abierto && (
        <ul
          role="menu"
          style={{
            position: 'absolute', right: 0, top: '100%', zIndex: 10,
            background: 'var(--surface)', border: '1px solid var(--border)',
            borderRadius: 'var(--r-8)', listStyle: 'none', padding: 'var(--s-5)', margin: 0,
            minWidth: 180, boxShadow: '0 2px 8px rgba(0,0,0,.12)',
          }}
        >
          {!esPropiaFila && usuario.estado !== 'Desactivada' && (
            <li role="menuitem" onClick={() => accion(() => setDialogo('rol'))}
              style={{ padding: 'var(--s-8) var(--s-13)', cursor: 'pointer' }}>
              {T.cambiarRol}
            </li>
          )}
          {!esPropiaFila && usuario.estado !== 'Desactivada' && (
            <li role="menuitem" onClick={() => accion(() => setDialogo('desactivar'))}
              style={{ padding: 'var(--s-8) var(--s-13)', cursor: 'pointer' }}>
              {T.desactivar}
            </li>
          )}
          {usuario.estado === 'Desactivada' && (
            <li role="menuitem" onClick={() => accion(() => activar.mutate())}
              style={{ padding: 'var(--s-8) var(--s-13)', cursor: 'pointer' }}>
              {T.activar}
            </li>
          )}
          {puedeReenviar && (
            <li role="menuitem" onClick={() => accion(() => reenviar.mutate())}
              style={{ padding: 'var(--s-8) var(--s-13)', cursor: 'pointer' }}>
              {usuario.estado === 'InvitacionPendiente' ? T.reenviarInvitacion : T.reenviarVerificacion}
            </li>
          )}
        </ul>
      )}

      {dialogo === 'rol' && (
        <DialogoRol
          usuarioId={usuario.id}
          rolActual={usuario.rol}
          onCerrar={cerrarTodo}
        />
      )}
      {dialogo === 'desactivar' && (
        <DialogoDesactivar
          usuarioId={usuario.id}
          onCerrar={cerrarTodo}
        />
      )}
    </div>
  )
}
