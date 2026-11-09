// T044 — useDetalleAnomalia (data-model §3)
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import { armarError } from '@/api/problema'
import type { DetalleAnomalia } from '@/api/dtos/002-mapa'

export function useDetalleAnomalia(id: string | null) {
  return useQuery({
    queryKey: ['anomalia', id],
    queryFn: async () => {
      if (!id) return null
      const r = await fetch(`${entorno.urlApi}/api/v1/anomalies/${id}`, {
        credentials: 'include',
      })
      if (!r.ok) throw await armarError(r)
      return r.json() as Promise<DetalleAnomalia>
    },
    enabled: id !== null,
    staleTime: 30_000,
  })
}
