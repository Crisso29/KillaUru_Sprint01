// T044 — useMapaAdmin y useMapaAdminFranja (data-model §3)
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import { armarError } from '@/api/problema'
import type { ColeccionAnomalias } from '@/api/dtos/002-mapa'

export function useMapaAdmin(consulta: URLSearchParams | null) {
  return useQuery({
    queryKey: ['mapa', consulta?.toString() ?? ''],
    queryFn: async () => {
      if (!consulta) return null
      const r = await fetch(`${entorno.urlApi}/api/v1/anomalies?${consulta.toString()}`, {
        credentials: 'include',
      })
      if (!r.ok) throw await armarError(r)
      return r.json() as Promise<ColeccionAnomalias>
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
      const r = await fetch(`${entorno.urlApi}/api/v1/anomalies?estado=Activa`, {
        credentials: 'include',
      })
      if (!r.ok) throw await armarError(r)
      return r.json() as Promise<ColeccionAnomalias>
    },
    staleTime: 30_000,
    refetchInterval: 60_000,
  })
}
