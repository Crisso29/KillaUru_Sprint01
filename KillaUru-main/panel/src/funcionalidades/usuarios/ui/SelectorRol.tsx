// T057 — lista de roles (RN-07); la reusan invitar y cambiar rol
import type { Rol } from '@/api/dtos/001-identidad'
import { TEXTOS_USUARIOS as textos } from '../textos'

export const NOMBRE_ROL: Record<Rol, string> = { ConductorRegistrado: 'Conductor registrado', Administrador: 'Administrador' }

interface Props { valor: Rol; onCambiar: (rol: Rol) => void; orden: Rol[] }

export function SelectorRol({ valor, onCambiar, orden }: Props) {
  return (
    <label>
      <span>{textos.rolLabel}</span>
      <select aria-label={textos.rolLabel} value={valor} onChange={(evento) => onCambiar(evento.target.value as Rol)}>
        {orden.map((rol) => <option key={rol} value={rol}>{NOMBRE_ROL[rol]}</option>)}
      </select>
    </label>
  )
}
