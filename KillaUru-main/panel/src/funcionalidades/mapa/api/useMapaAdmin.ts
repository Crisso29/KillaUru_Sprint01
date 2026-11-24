// T044 — useMapaAdmin y useMapaAdminFranja (data-model §3)
import { useQuery } from '@tanstack/react-query'
import { armarError } from '@/api/problema'
import type { ColeccionAnomalias } from '@/api/dtos/002-mapa'
import { pedirApi } from '@/api/http'

export function useMapaAdmin(consulta: URLSearchParams | null) {
  return useQuery({
    queryKey: ['mapa', consulta?.toString() ?? ''],
    queryFn: async () => {
      if (!consulta) return null
      const respuesta = await pedirApi(`/api/v1/anomalies?${consulta.toString()}`)
      if (!respuesta.ok) throw await armarError(respuesta)
      return respuesta.json() as Promise<ColeccionAnomalias>
    },
    enabled: consulta !== null,
    staleTime: 30_000,
    refetchInterval: 60_000,
  })
}

export function useMapaAdminFranja() {
  return useQuery({
    queryKey: ['mapa', 'franja'],
    queryFn: async () => {
      const respuesta = await pedirApi(`/api/v1/anomalies?estado=Activa`)
      if (!respuesta.ok) throw await armarError(respuesta)
      return respuesta.json() as Promise<ColeccionAnomalias>
    },
    staleTime: 30_000,
    refetchInterval: 60_000,
  })
}
