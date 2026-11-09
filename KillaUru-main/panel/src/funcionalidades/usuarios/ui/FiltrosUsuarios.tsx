// T056 — FiltrosUsuarios: búsqueda, rol y estado (FR-031)
import type { Rol, EstadoUsuario } from '@/api/dtos/001-identidad'
import { TEXTOS_USUARIOS as T } from '../textos'

interface Props {
  texto: string
  rol: Rol | ''
  estado: EstadoUsuario | ''
  onChange: (campo: 'texto' | 'rol' | 'estado', valor: string) => void
  onBuscar: () => void
}

export function FiltrosUsuarios({ texto, rol, estado, onChange, onBuscar }: Props) {
  return (
    <div style={{ display: 'flex', gap: 'var(--s-13)', flexWrap: 'wrap', marginBottom: 'var(--s-21)' }}>
      <label style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-5)', flex: 1, minWidth: 200 }}>
        <span style={{ fontSize: 'var(--t-13)', fontWeight: 500 }}>{T.labelBuscar}</span>
        <input
          type="search"
          role="searchbox"
          aria-label={T.labelBuscar}
          value={texto}
          placeholder={T.buscarPlaceholder}
          onChange={e => onChange('texto', e.target.value)}
          onKeyDown={e => e.key === 'Enter' && onBuscar()}
          style={{ padding: 'var(--s-8) var(--s-13)', border: '1px solid var(--border)', borderRadius: 'var(--r-8)' }}
        />
      </label>

      <label style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-5)' }}>
        <span style={{ fontSize: 'var(--t-13)', fontWeight: 500 }}>{T.labelRol}</span>
        <select
          aria-label={T.labelRol}
          value={rol}
          onChange={e => onChange('rol', e.target.value)}
          style={{ padding: 'var(--s-8) var(--s-13)', border: '1px solid var(--border)', borderRadius: 'var(--r-8)' }}
        >
          <option value="">{T.todoRol}</option>
          <option value="Administrador">Administrador</option>
          <option value="ConductorRegistrado">Conductor registrado</option>
        </select>
      </label>

      <label style={{ display: 'flex', flexDirection: 'column', gap: 'var(--s-5)' }}>
        <span style={{ fontSize: 'var(--t-13)', fontWeight: 500 }}>{T.labelEstado}</span>
        <select
          aria-label={T.labelEstado}
          value={estado}
          onChange={e => onChange('estado', e.target.value)}
          style={{ padding: 'var(--s-8) var(--s-13)', border: '1px solid var(--border)', borderRadius: 'var(--r-8)' }}
        >
          <option value="">{T.todoEstado}</option>
          <option value="Activa">Activa</option>
          <option value="Desactivada">Desactivada</option>
          <option value="InvitacionPendiente">Invitación pendiente</option>
        </select>
      </label>
    </div>
  )
}
