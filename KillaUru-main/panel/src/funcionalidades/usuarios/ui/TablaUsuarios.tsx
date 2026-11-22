// T056 — TablaUsuarios: cuentas con su menú por fila (FR-031; prototipo tabla `.dt` con avatar)
import type { Usuario } from '@/api/dtos/001-identidad'
import { iniciales } from '@/compartido/formato/iniciales'
import { formatearFechaLima } from '@/compartido/formato/fechas'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { relojReal } from '@/compartido/reloj'
import { MenuUsuario } from './MenuUsuario'

interface Props {
  usuarios: Usuario[]
  correoActual: string
}

const ESTADO: Record<string, { texto: string; clase: string }> = {
  Activa: { texto: 'Activa', clase: 'activa' },
  Desactivada: { texto: 'Desactivada', clase: 'expirada' },
  InvitacionPendiente: { texto: 'Invitación pendiente', clase: 'pendiente' },
}

const ROL: Record<string, string> = { ConductorRegistrado: 'Conductor registrado', Administrador: 'Administrador' }

export function TablaUsuarios({ usuarios, correoActual }: Props) {
  const ahora = relojReal.ahora()
  return (
    <table className="dt">
      <thead>
        <tr>
          <th scope="col">Usuario</th>
          <th scope="col">Rol</th>
          <th scope="col">Estado</th>
          <th scope="col">Correo verificado</th>
          <th scope="col">Alta</th>
          <th scope="col">Última sesión</th>
          <th scope="col"><span className="sr-only">Acciones</span></th>
        </tr>
      </thead>
      <tbody>
        {usuarios.map(u => {
          const estado = ESTADO[u.estado] ?? { texto: u.estado, clase: 'expirada' }
          return (
            <tr key={u.id}>
              <td>
                <span style={{ display: 'flex', gap: 'var(--s-8)', alignItems: 'center' }}>
                  <span className="av" aria-hidden="true">{iniciales(u.correo)}</span>
                  <span><b style={{ display: 'block' }}>{u.correo}</b><span className="mono">{u.id.slice(0, 8)}</span></span>
                </span>
              </td>
              <td>{ROL[u.rol] ?? u.rol}</td>
              <td><span className={`sd ${estado.clase}`}><span>{estado.texto}</span></span></td>
              <td style={{ color: u.correoVerificado ? undefined : 'var(--ink-3)' }}>{u.correoVerificado ? 'Sí' : 'Pendiente'}</td>
              <td>{formatearFechaLima(u.fechaAltaUtc)}</td>
              <td>{u.ultimaSesionUtc ? formatearRelativo(u.ultimaSesionUtc, ahora) : '—'}</td>
              <td className="r"><MenuUsuario usuario={u} correoActual={correoActual} /></td>
            </tr>
          )
        })}
      </tbody>
    </table>
  )
}
