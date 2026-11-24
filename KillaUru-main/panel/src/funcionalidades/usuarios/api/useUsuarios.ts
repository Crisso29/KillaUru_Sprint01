// T055 — useUsuarios: listado paginado con key ['usuarios', consulta] staleTime 10 min
import { useQuery } from '@tanstack/react-query'
import type { PaginaUsuarios } from '@/api/dtos/001-identidad'
import { pedirApi } from '@/api/http'

export function useUsuarios(consulta: URLSearchParams) {
  return useQuery<PaginaUsuarios>({
    queryKey: ['usuarios', consulta.toString()],
    queryFn: async () => {
      const respuesta = await pedirApi(
        `/api/v1/usuarios?${consulta}`
      )
      if (!respuesta.ok) throw new Error('error_red')
      return respuesta.json() as Promise<PaginaUsuarios>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })
}
