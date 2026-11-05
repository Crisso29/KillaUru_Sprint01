// T055 — mutaciones de gestión de usuarios con invalidaciones ['usuarios','revision','actores']
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'
import type { Rol, Usuario } from '@/api/generado/001-identidad'

const LLAVES_INVALIDAS = [
  { queryKey: ['usuarios'] },
  { queryKey: ['revision'] },
  { queryKey: ['actores'] },
]

function invalidarTodo(qc: ReturnType<typeof useQueryClient>) {
  LLAVES_INVALIDAS.forEach(k => qc.invalidateQueries(k))
}

async function fetchAccion(url: string, method: string, body?: unknown): Promise<void> {
  const r = await fetch(url, {
    method,
    credentials: 'include',
    ...(body ? { headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) } : {}),
  })
  if (!r.ok) {
    const j = await r.json() as { codigo: string }
    throw j
  }
}

export function useCambiarRol(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, { rol: Rol }>({
    mutationFn: (body) =>
      fetchAccion(`${entorno.urlApi}/api/v1/usuarios/${usuarioId}/rol`, 'PUT', body),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useDesactivarUsuario(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, { motivo: string }>({
    mutationFn: (body) =>
      fetchAccion(`${entorno.urlApi}/api/v1/usuarios/${usuarioId}/desactivar`, 'POST', body),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useActivarUsuario(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, void>({
    mutationFn: () =>
      fetchAccion(`${entorno.urlApi}/api/v1/usuarios/${usuarioId}/activar`, 'POST'),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useReenviarCorreo(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, void>({
    mutationFn: () =>
      fetchAccion(`${entorno.urlApi}/api/v1/usuarios/${usuarioId}/reenviar-correo`, 'POST'),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useCrearUsuario() {
  const qc = useQueryClient()
  return useMutation<Usuario, { codigo: string }, { correo: string; rol: Rol }>({
    mutationFn: async (body) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/usuarios`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      if (!r.ok) {
        const j = await r.json() as { codigo: string }
        throw j
      }
      return r.json() as Promise<Usuario>
    },
    onSuccess: () => invalidarTodo(qc),
  })
}
