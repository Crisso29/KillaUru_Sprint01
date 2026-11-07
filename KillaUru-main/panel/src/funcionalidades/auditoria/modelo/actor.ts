import type { Usuario } from '@/api/generado/001-identidad'

const UUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
const CORREO_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export type TipoActor =
  | { tipo: 'id'; valor: string }
  | { tipo: 'correo'; valor: string }
  | { tipo: 'invalido' }

export function detectarTipoActor(texto: string): TipoActor {
  if (UUID_RE.test(texto)) return { tipo: 'id', valor: texto }
  if (CORREO_RE.test(texto)) return { tipo: 'correo', valor: texto }
  return { tipo: 'invalido' }
}

export function elegirCuenta(busqueda: string, usuarios: Usuario[]): Usuario | null {
  const normalizado = busqueda.toLowerCase()
  const exactos = usuarios.filter(u => u.correo.toLowerCase() === normalizado)
  if (exactos.length === 1) return exactos[0]
  return null
}
