// T066 — listas de moderación: por-revisar y pendientes (FR-028, FR-029)
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { ElementoPorRevisar, ElementoPendiente } from '@/api/generado/003-anomalias'

export function usePorRevisar() {
  return useQuery<ElementoPorRevisar[]>({
    queryKey: ['por-revisar'],
    queryFn: async () => {
      const r = await fetch(`${entorno.urlApi}/api/v1/moderacion/por-revisar`, {
        credentials: 'include',
      })
      if (!r.ok) throw await r.json()
      return r.json() as Promise<ElementoPorRevisar[]>
    },
    staleTime: 30_000,
    refetchInterval: 60_000,
    refetchOnWindowFocus: true,
  })
}

export function usePendientes() {
  return useQuery<ElementoPendiente[]>({
    queryKey: ['pendientes'],
    queryFn: async () => {
      const r = await fetch(`${entorno.urlApi}/api/v1/moderacion/pendientes`, {
        credentials: 'include',
      })
      if (!r.ok) throw await r.json()
      return r.json() as Promise<ElementoPendiente[]>
    },
    staleTime: 30_000,
    refetchInterval: 60_000,
    refetchOnWindowFocus: true,
  })
}
