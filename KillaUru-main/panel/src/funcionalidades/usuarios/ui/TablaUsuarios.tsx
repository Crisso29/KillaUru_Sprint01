// T056 — TablaUsuarios: cuentas con su menú por fila (FR-031; prototipo tabla `.dt` con avatar)
import type { Usuario } from '@/api/dtos/001-identidad'
import { iniciales } from '@/compartido/formato/iniciales'
import { formatearFechaLima } from '@/compartido/formato/fechas'
import { formatearRelativo } from '@/compartido/formato/relativo'
import { relojReal } from '@/compartido/reloj'
import { MenuUsuario } from './MenuUsuario'
import { NOMBRE_ROL } from './SelectorRol'

interface Props {
  usuarios: Usuario[]
  correoActual: string
}

const ESTADO: Record<string, { texto: string; clase: string }> = {
  Activa: { texto: 'Activa', clase: 'activa' },
  Desactivada: { texto: 'Desactivada', clase: 'expirada' },
  InvitacionPendiente: { texto: 'Invitación pendiente', clase: 'pendiente' },
}

const COLUMNAS = ['Usuario', 'Rol', 'Estado', 'Correo verificado', 'Alta', 'Última sesión']

export function TablaUsuarios({ usuarios, correoActual }: Props) {
  const ahora = relojReal.ahora()
  return (
    <table className="dt">
      <thead>
        <tr>
          {COLUMNAS.map((columna) => <th key={columna} scope="col">{columna}</th>)}
          <th scope="col"><span className="sr-only">Acciones</span></th>
        </tr>
      </thead>
      <tbody>
        {usuarios.map((usuario) => <FilaUsuario key={usuario.id} usuario={usuario} correoActual={correoActual} ahora={ahora} />)}
      </tbody>
    </table>
  )
}

function FilaUsuario({ usuario, correoActual, ahora }: { usuario: Usuario; correoActual: string; ahora: Date }) {
  const estado = ESTADO[usuario.estado] ?? { texto: usuario.estado, clase: 'expirada' }
  return (
    <tr>
      <td>
        <span style={{ display: 'flex', gap: 'var(--s-8)', alignItems: 'center' }}>
          <span className="av" aria-hidden="true">{iniciales(usuario.correo)}</span>
          <span><b style={{ display: 'block' }}>{usuario.correo}</b><span className="mono">{usuario.id.slice(0, 8)}</span></span>
        </span>
      </td>
      <td>{NOMBRE_ROL[usuario.rol] ?? usuario.rol}</td>
      <td><span className={`sd ${estado.clase}`}><span>{estado.texto}</span></span></td>
      <td style={{ color: usuario.correoVerificado ? undefined : 'var(--ink-3)' }}>{usuario.correoVerificado ? 'Sí' : 'Pendiente'}</td>
      <td>{formatearFechaLima(usuario.fechaAltaUtc)}</td>
      <td>{usuario.ultimaSesionUtc ? formatearRelativo(usuario.ultimaSesionUtc, ahora) : '—'}</td>
      <td className="r"><MenuUsuario usuario={usuario} correoActual={correoActual} /></td>
    </tr>
  )
}
