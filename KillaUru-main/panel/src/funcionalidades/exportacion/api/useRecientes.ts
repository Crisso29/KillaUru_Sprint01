// T084 — useRecientes: GET /api/v1/auditoria?accion=DatasetExportado
import { useQuery } from '@tanstack/react-query'
import { pedirApi } from '@/api/http'

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
      const respuesta = await pedirApi(
        `/api/v1/auditoria?accion=DatasetExportado`
      )
      if (!respuesta.ok) return []
      const json = await respuesta.json() as { elementos: RegistroExportacion[] }
      return json.elementos ?? []
    },
    staleTime: 2 * 60_000,
  })
}
