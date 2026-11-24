// T044 — useDetalleAnomalia (data-model §3)
import { useQuery } from '@tanstack/react-query'
import { armarError } from '@/api/problema'
import type { DetalleAnomalia } from '@/api/dtos/002-mapa'
import { pedirApi } from '@/api/http'

export function useDetalleAnomalia(id: string | null) {
  return useQuery({
    queryKey: ['anomalia', id],
    queryFn: async () => {
      if (!id) return null
      const respuesta = await pedirApi(`/api/v1/anomalies/${id}`)
      if (!respuesta.ok) throw await armarError(respuesta)
      return respuesta.json() as Promise<DetalleAnomalia>
    },
    enabled: id !== null,
    staleTime: 30_000,
  })
}
