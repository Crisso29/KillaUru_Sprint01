// T074 — useIndicadores: ['indicadores', dias], staleTime 10 min (data-model §3, FR-015)
import { useQuery } from '@tanstack/react-query'
import type { ResumenDashboard } from '@/api/dtos/004-dashboard'
import { pedirApi } from '@/api/http'

export function useIndicadores(dias: 7 | 14 | 30 = 14) {
  return useQuery<ResumenDashboard>({
    queryKey: ['indicadores', dias],
    queryFn: async () => {
      const respuesta = await pedirApi(
        `/api/v1/dashboard/indicadores?dias=${dias}`
      )
      if (!respuesta.ok) throw new Error('error_red')
      return respuesta.json() as Promise<ResumenDashboard>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: true,
  })
}
