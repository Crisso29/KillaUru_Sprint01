// T066 — listas de moderación: por-revisar y pendientes (FR-028, FR-029). Datos operativos: se refrescan cada minuto.
import { useQuery } from '@tanstack/react-query'
import type { ElementoPorRevisar, ElementoPendiente } from '@/api/dtos/003-anomalias'
import { pedirJson } from '@/api/http'

function useListaModeracion<T>(llave: string) {
  return useQuery<T[]>({
    queryKey: [llave],
    queryFn: () => pedirJson<T[]>(`/api/v1/moderacion/${llave}`),
    staleTime: 30_000,
    refetchInterval: 60_000,
    refetchOnWindowFocus: true,
  })
}

export const usePorRevisar = () => useListaModeracion<ElementoPorRevisar>('por-revisar')

export const usePendientes = () => useListaModeracion<ElementoPendiente>('pendientes')
