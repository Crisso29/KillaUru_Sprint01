// T055 — useRevision: estado actual + mutación registrar (key ['revision'] staleTime 10 min)
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { EstadoRevision } from '@/api/dtos/001-identidad'

export function useRevision() {
  return useQuery<EstadoRevision>({
    queryKey: ['revision'],
    queryFn: async () => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/usuarios/revision-privilegios`,
        { credentials: 'include' },
      )
      if (!r.ok) throw new Error('error_red')
      return r.json() as Promise<EstadoRevision>
    },
    staleTime: 10 * 60_000,
    refetchOnWindowFocus: false,
  })
}

export function useRegistrarRevision() {
  const qc = useQueryClient()
  return useMutation<EstadoRevision, { codigo: string }, { confirmados: string[]; retirados: string[] }>({
    mutationFn: async (body) => {
      const r = await fetch(
        `${entorno.urlApi}/api/v1/usuarios/revision-privilegios`,
        { method: 'POST', credentials: 'include', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) },
      )
      if (!r.ok) {
        const j = await r.json() as { codigo: string }
        throw j
      }
      return r.json() as Promise<EstadoRevision>
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['revision'] })
      qc.invalidateQueries({ queryKey: ['usuarios'] })
    },
  })
}
