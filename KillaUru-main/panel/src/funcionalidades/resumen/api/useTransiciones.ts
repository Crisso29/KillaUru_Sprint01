// T074 — useTransiciones: ['transiciones', n], staleTime 30 s, refresco 60 s (data-model §3)
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { TransicionReciente } from '@/api/generado/003-anomalias'

export function useTransiciones(limite = 5) {
  return useQuery<TransicionReciente[]>({
    queryKey: ['transiciones', limite],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/anomalies/transiciones?limite=${limite}`,
        { credentials: 'include' },
      )
      if (!r.ok) throw new Error('error_red')
      return r.json() as Promise<TransicionReciente[]>
    },
    staleTime: 30_000,
    refetchInterval: 60_000,
    refetchOnWindowFocus: true,
  })
}
