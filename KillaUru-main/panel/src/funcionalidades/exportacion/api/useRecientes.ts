// T084 — useRecientes: GET /api/v1/auditoria?accion=DatasetExportado
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'

export interface RegistroExportacion {
  id: number
  ocurridoUtc: string
  actorId: string | null
  contexto: Record<string, unknown>
}

export function useRecientes() {
  return useQuery<RegistroExportacion[]>({
    queryKey: ['exportaciones-recientes'],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/auditoria?accion=DatasetExportado`,
        { credentials: 'include' },
      )
      if (!r.ok) return []
      const json = await r.json() as { elementos: RegistroExportacion[] }
      return json.elementos ?? []
    },
    staleTime: 2 * 60_000,
  })
}
