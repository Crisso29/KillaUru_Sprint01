// T056 — TablaUsuarios: lista de usuarios con MenuUsuario por fila
import type { Usuario } from '@/api/dtos/001-identidad'
import { MenuUsuario } from './MenuUsuario'

interface Props {
  usuarios: Usuario[]
  correoActual: string
}

function etiquetaEstado(estado: string) {
  if (estado === 'InvitacionPendiente') return 'Invitación pendiente'
  return estado
}

function etiquetaRol(rol: string) {
  if (rol === 'ConductorRegistrado') return 'Conductor'
  return rol
}

export function TablaUsuarios({ usuarios, correoActual }: Props) {
  return (
    <div style={{ overflowX: 'auto' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 'var(--t-13)' }}>
        <thead>
          <tr style={{ borderBottom: '2px solid var(--border)', textAlign: 'left' }}>
            <th style={{ padding: 'var(--s-8) var(--s-13)', fontWeight: 600 }}>Correo</th>
            <th style={{ padding: 'var(--s-8) var(--s-13)', fontWeight: 600 }}>Rol</th>
            <th style={{ padding: 'var(--s-8) var(--s-13)', fontWeight: 600 }}>Estado</th>
            <th style={{ padding: 'var(--s-8) var(--s-13)', fontWeight: 600 }}>Alta</th>
            <th style={{ padding: 'var(--s-8) var(--s-13)', width: 48 }}></th>
          </tr>
        </thead>
        <tbody>
          {usuarios.map(u => (
            <tr key={u.id} style={{ borderBottom: '1px solid var(--border)' }}>
              <td style={{ padding: 'var(--s-8) var(--s-13)' }}>{u.correo}</td>
              <td style={{ padding: 'var(--s-8) var(--s-13)' }}>{etiquetaRol(u.rol)}</td>
              <td style={{ padding: 'var(--s-8) var(--s-13)' }}>{etiquetaEstado(u.estado)}</td>
              <td style={{ padding: 'var(--s-8) var(--s-13)', color: 'var(--ink-3)' }}>
                {u.fechaAltaUtc.slice(0, 10)}
              </td>
              <td style={{ padding: 'var(--s-8) var(--s-5)' }}>
                <MenuUsuario usuario={u} correoActual={correoActual} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
