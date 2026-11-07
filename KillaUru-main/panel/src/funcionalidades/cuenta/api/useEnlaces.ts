// T061 — hooks para los endpoints de enlace de correo (FR-014)
import { useMutation } from '@tanstack/react-query'
import { entorno } from '@/config/entorno'

export function useVerificarCorreo() {
  return useMutation<void, { codigo: string }, string>({
    mutationFn: async (token) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/auth/verificar-correo`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token }),
      })
      if (!r.ok) {
        const j = await r.json() as { codigo: string }
        throw j
      }
    },
  })
}

export function useFijarContrasena() {
  return useMutation<void, { codigo: string }, { token: string; nuevaContrasena: string }>({
    mutationFn: async (body) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/auth/fijar-contrasena`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      if (!r.ok) {
        const j = await r.json() as { codigo: string }
        throw j
      }
    },
  })
}
