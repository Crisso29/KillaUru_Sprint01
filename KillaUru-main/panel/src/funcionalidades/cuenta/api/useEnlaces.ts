// T061 — hooks para los endpoints de enlace de correo (FR-014)
import { useMutation } from '@tanstack/react-query'
import { pedirApi } from '@/api/http'

export function useVerificarCorreo() {
  return useMutation<void, { codigo: string }, string>({
    mutationFn: async (token) => {
      const respuesta = await pedirApi(`/api/v1/auth/verificar-correo`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token }),
      })
      if (!respuesta.ok) {
        const j = await respuesta.json() as { codigo: string }
        throw j
      }
    },
  })
}

export function useFijarContrasena() {
  return useMutation<void, { codigo: string }, { token: string; nuevaContrasena: string }>({
    mutationFn: async (body) => {
      const respuesta = await pedirApi(`/api/v1/auth/fijar-contrasena`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })
      if (!respuesta.ok) {
        const j = await respuesta.json() as { codigo: string }
        throw j
      }
    },
  })
}
