// T074 — useTransiciones: ['transiciones', n], staleTime 30 s, refresco 60 s (data-model §3)
import { useQuery } from '@tanstack/react-query'
import type { TransicionReciente } from '@/api/dtos/003-anomalias'
import { pedirApi } from '@/api/http'

export function useTransiciones(limite = 5) {
  return useQuery<TransicionReciente[]>({
    queryKey: ['transiciones', limite],
    queryFn: async () => {
      const respuesta = await pedirApi(
        `/api/v1/anomalies/transiciones?limite=${limite}`
      )
      if (!respuesta.ok) throw new Error('error_red')
      return respuesta.json() as Promise<TransicionReciente[]>
    },
    staleTime: 30_000,
    refetchInterval: 60_000,
    refetchOnWindowFocus: true,
  })
}
