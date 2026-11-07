// T055 — useUsuarios: listado paginado con key ['usuarios', consulta] staleTime 10 min
import { useQuery } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { PaginaUsuarios } from '@/api/generado/001-identidad'

export function useUsuarios(consulta: URLSearchParams) {
  return useQuery<PaginaUsuarios>({
    queryKey: ['usuarios', consulta.toString()],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/usuarios?${consulta}`,
        { credentials: 'include' },
      )
      if (!r.ok) throw new Error('error_red')
      return r.json() as Promise<PaginaUsuarios>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })
}
