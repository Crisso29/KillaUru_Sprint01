// T055 — useRevision: estado actual + mutación registrar (key ['revision'] staleTime 10 min)
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { EstadoRevision } from '@/api/dtos/001-identidad'
import { pedirApi } from '@/api/http'

export function useRevision() {
  return useQuery<EstadoRevision>({
    queryKey: ['revision'],
    queryFn: async () => {
      const respuesta = await pedirApi(
        `/api/v1/usuarios/revision-privilegios`
      )
      if (!respuesta.ok) throw new Error('error_red')
      return respuesta.json() as Promise<EstadoRevision>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })
}

export function useRegistrarRevision() {
  const qc = useQueryClient()
  return useMutation<EstadoRevision, { codigo: string }, { confirmados: string[]; retirados: string[] }>({
    mutationFn: async (body) => {
      const respuesta = await pedirApi(
        `/api/v1/usuarios/revision-privilegios`,
        { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) },
      )
      if (!respuesta.ok) {
        const j = await respuesta.json() as { codigo: string }
        throw j
      }
      return respuesta.json() as Promise<EstadoRevision>
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['revision'] })
      qc.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}
