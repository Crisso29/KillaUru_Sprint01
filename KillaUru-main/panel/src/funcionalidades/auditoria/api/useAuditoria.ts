import { useQuery } from '@tanstack/react-query'
import type { PaginaAuditoria } from '@/api/dtos/002-auditoria'
import { pedirApi } from '@/api/http'

export interface FiltrosAuditoria {
  desde?: string
  hasta?: string
  categoria?: string
  accion?: string
  actorId?: string
  pagina?: number
}

export function useAuditoria(filtros: FiltrosAuditoria = {}) {
  const params = new URLSearchParams()
  if (filtros.desde) params.set('desde', filtros.desde)
  if (filtros.hasta) params.set('hasta', filtros.hasta)
  if (filtros.categoria) params.set('categoria', filtros.categoria)
  if (filtros.accion) params.set('accion', filtros.accion)
  if (filtros.actorId) params.set('actorId', filtros.actorId)
  params.set('pagina', String(filtros.pagina ?? 1))

  return useQuery<PaginaAuditoria>({
    queryKey: ['auditoria', params.toString()],
    queryFn: async () => {
      const respuesta = await pedirApi(`/api/v1/auditoria?${params}`)
      if (!respuesta.ok) throw new Error('error_red')
      return respuesta.json() as Promise<PaginaAuditoria>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })
}
