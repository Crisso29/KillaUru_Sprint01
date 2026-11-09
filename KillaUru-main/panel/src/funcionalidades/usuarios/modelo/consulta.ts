// T055 — aConsulta: construye URLSearchParams a partir de texto, rol, estado y pagina
import type { Rol, EstadoUsuario } from '@/api/dtos/001-identidad'

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

export function aConsulta(
  texto: string,
  rol: Rol | '',
  estado: EstadoUsuario | '',
  pagina: number,
): URLSearchParams {
  const p = new URLSearchParams()

  if (UUID_RE.test(texto)) {
    p.set('ids', texto)
  } else if (texto.length >= 3) {
    p.set('busqueda', texto)
  }

  if (rol) p.set('rol', rol)
  if (estado) p.set('estado', estado)
  p.set('pagina', String(pagina))

  return p
}
