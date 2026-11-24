// T080, R-015 — el filtro «actor» acepta un id o un correo; la auditoría filtra por id. Si se escribió un correo, se
// busca la cuenta (`GET /usuarios?busqueda=`) y se toma la que coincide exactamente (`elegirCuenta`).
import { useQuery } from '@tanstack/react-query'
import { pedirJson } from '@/api/http'
import type { PaginaUsuarios } from '@/api/dtos/001-identidad'
import { detectarTipoActor, elegirCuenta } from '../modelo/actor'

export interface ActorFiltrado {
  /** Id para filtrar; undefined = sin filtro de actor (vacío, inválido o correo aún sin resolver). */
  actorId?: string
  /** Se escribió un correo, la búsqueda terminó y ninguna cuenta coincide: se avisa en vez de mostrar todo. */
  sinCuenta: boolean
}

export function useActorFiltrado(texto: string): ActorFiltrado {
  const tipo = detectarTipoActor(texto)
  const { data: encontrados } = useQuery({
    queryKey: ['actores', 'busqueda', texto],
    queryFn: () => pedirJson<PaginaUsuarios>(`/api/v1/usuarios?busqueda=${encodeURIComponent(texto)}`),
    enabled: tipo.tipo === 'correo',
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })
  if (tipo.tipo === 'id') return { actorId: tipo.valor, sinCuenta: false }
  if (tipo.tipo !== 'correo' || !encontrados) return { sinCuenta: false }
  const cuenta = elegirCuenta(texto, encontrados.elementos)
  return { actorId: cuenta?.id, sinCuenta: cuenta === null }
}
