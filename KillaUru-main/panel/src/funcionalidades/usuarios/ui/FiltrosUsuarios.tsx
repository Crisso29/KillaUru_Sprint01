// T056 — FiltrosUsuarios: búsqueda, rol y estado (FR-031; prototipo `.toolbar`)
import type { Rol, EstadoUsuario } from '@/api/dtos/001-identidad'
import { Icono } from '@/compartido/ui'
import { TEXTOS_USUARIOS as textos } from '../textos'

type Campo = 'texto' | 'rol' | 'estado'

interface Props {
  texto: string
  rol: Rol | ''
  estado: EstadoUsuario | ''
  onChange: (campo: Campo, valor: string) => void
  onBuscar: () => void
}

const ROLES = [['', textos.todoRol], ['Administrador', 'Administrador'], ['ConductorRegistrado', 'Conductor registrado']]
const ESTADOS = [['', textos.todoEstado], ['Activa', 'Activa'], ['Desactivada', 'Desactivada'], ['InvitacionPendiente', 'Invitación pendiente']]

/** Las etiquetas quedan para lectores de pantalla (`aria-label`); a la vista, como en el prototipo, solo los campos. */
export function FiltrosUsuarios({ texto, rol, estado, onChange, onBuscar }: Props) {
  return (
    <div className="toolbar" role="search" aria-label="Filtros de usuarios">
      <label className="search" style={{ margin: 0, maxWidth: 280, height: 30, background: 'var(--surface)', borderColor: 'var(--line-2)' }}>
        <Icono nombre="search" tamano={14} />
        <input type="search" role="searchbox" aria-label={textos.labelBuscar} value={texto} placeholder={textos.buscarPlaceholder}
          onChange={(evento) => onChange('texto', evento.target.value)} onKeyDown={(evento) => evento.key === 'Enter' && onBuscar()} />
      </label>
      <Desplegable etiqueta={textos.labelRol} valor={rol} opciones={ROLES} onCambiar={(valor) => onChange('rol', valor)} />
      <Desplegable etiqueta={textos.labelEstado} valor={estado} opciones={ESTADOS} onCambiar={(valor) => onChange('estado', valor)} />
    </div>
  )
}

interface PropsDesplegable { etiqueta: string; valor: string; opciones: string[][]; onCambiar: (valor: string) => void }

function Desplegable({ etiqueta, valor, opciones, onCambiar }: PropsDesplegable) {
  return (
    <select className="tsel" aria-label={etiqueta} value={valor} onChange={(evento) => onCambiar(evento.target.value)}>
      {opciones.map(([clave, texto]) => <option key={clave} value={clave}>{texto}</option>)}
    </select>
  )
}
