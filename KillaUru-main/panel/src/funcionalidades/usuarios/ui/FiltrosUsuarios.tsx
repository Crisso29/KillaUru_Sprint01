// T056 — FiltrosUsuarios: búsqueda, rol y estado (FR-031; prototipo `.toolbar`)
import type { Rol, EstadoUsuario } from '@/api/dtos/001-identidad'
import { Icono } from '@/compartido/ui'
import { TEXTOS_USUARIOS as T } from '../textos'

interface Props {
  texto: string
  rol: Rol | ''
  estado: EstadoUsuario | ''
  onChange: (campo: 'texto' | 'rol' | 'estado', valor: string) => void
  onBuscar: () => void
}

/** Las etiquetas quedan para lectores de pantalla (`aria-label`); a la vista, como en el prototipo, solo los campos. */
export function FiltrosUsuarios({ texto, rol, estado, onChange, onBuscar }: Props) {
  return (
    <div className="toolbar" role="search" aria-label="Filtros de usuarios">
      <label className="search" style={{ margin: 0, maxWidth: 280, height: 30, background: 'var(--surface)', borderColor: 'var(--line-2)' }}>
        <Icono nombre="search" tamano={14} />
        <input
          type="search"
          role="searchbox"
          aria-label={T.labelBuscar}
          value={texto}
          placeholder={T.buscarPlaceholder}
          onChange={e => onChange('texto', e.target.value)}
          onKeyDown={e => e.key === 'Enter' && onBuscar()}
        />
      </label>
      <select className="tsel" aria-label={T.labelRol} value={rol} onChange={e => onChange('rol', e.target.value)}>
        <option value="">{T.todoRol}</option>
        <option value="Administrador">Administrador</option>
        <option value="ConductorRegistrado">Conductor registrado</option>
      </select>
      <select className="tsel" aria-label={T.labelEstado} value={estado} onChange={e => onChange('estado', e.target.value)}>
        <option value="">{T.todoEstado}</option>
        <option value="Activa">Activa</option>
        <option value="Desactivada">Desactivada</option>
        <option value="InvitacionPendiente">Invitación pendiente</option>
      </select>
    </div>
  )
}
