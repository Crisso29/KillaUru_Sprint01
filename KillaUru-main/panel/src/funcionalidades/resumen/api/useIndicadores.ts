// T074 — useIndicadores: ['indicadores', dias], staleTime 10 min (data-model §3, FR-015)
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { ResumenDashboard } from '@/api/generado/004-dashboard'

export function useIndicadores(dias: 7 | 14 | 30 = 14) {
  return useQuery<ResumenDashboard>({
    queryKey: ['indicadores', dias],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/dashboard/indicadores?dias=${dias}`,
        { credentials: 'include' },
      )
      if (!r.ok) throw new Error('error_red')
      return r.json() as Promise<ResumenDashboard>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: true,
  })
}
