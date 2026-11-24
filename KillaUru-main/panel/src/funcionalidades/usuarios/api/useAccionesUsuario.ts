// T055 — mutaciones de gestión de usuarios con invalidaciones ['usuarios','revision','actores']
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { pedirApi } from '@/api/http'
import { armarError } from '@/api/problema'
import type { Rol, Usuario } from '@/api/dtos/001-identidad'

const LLAVES_INVALIDAS = [
  { queryKey: ['usuarios'] },
  { queryKey: ['revision'] },
  { queryKey: ['actores'] },
]

function invalidarTodo(qc: ReturnType<typeof useQueryClient>) {
  LLAVES_INVALIDAS.forEach(llave => qc.invalidateQueries(llave))
}

/** Acción sobre una cuenta; si falla lanza el ErrorApi del Problema (su `codigo` decide el mensaje). */
async function ejecutarAccion(ruta: string, method: string, body?: unknown): Promise<void> {
  const respuesta = await pedirApi(ruta, {
    method,
    ...(body ? { headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) } : {}),
  })
  if (!respuesta.ok) throw await armarError(respuesta)
}

export function useCambiarRol(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, { rol: Rol }>({
    mutationFn: (body) =>
      ejecutarAccion(`/api/v1/usuarios/${usuarioId}/rol`, 'PUT', body),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useDesactivarUsuario(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, { motivo: string }>({
    mutationFn: (body) =>
      ejecutarAccion(`/api/v1/usuarios/${usuarioId}/desactivar`, 'POST', body),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useActivarUsuario(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, void>({
    mutationFn: () =>
      ejecutarAccion(`/api/v1/usuarios/${usuarioId}/activar`, 'POST'),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useReenviarCorreo(usuarioId: string) {
  const qc = useQueryClient()
  return useMutation<void, { codigo: string }, void>({
    mutationFn: () =>
      ejecutarAccion(`/api/v1/usuarios/${usuarioId}/reenviar-correo`, 'POST'),
    onSuccess: () => invalidarTodo(qc),
  })
}

export function useCrearUsuario() {
  const qc = useQueryClient()
  return useMutation<Usuario, { codigo: string }, { correo: string; rol: Rol }>({
    mutationFn: async (body) => {
      const respuesta = await pedirApi('/api/v1/usuarios', {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body),
      })
      if (!respuesta.ok) throw await armarError(respuesta)
      return await respuesta.json() as Usuario
    },
    onSuccess: () => invalidarTodo(qc),
  })
}
