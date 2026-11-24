// T031 — mutación para solicitar recuperación de contraseña (US1, FR-013)
import { useMutation } from '@tanstack/react-query'
import { armarError } from '@/api/problema'
import { pedirApi } from '@/api/http'

export function useRecuperar() {
  return useMutation({
    mutationFn: async (correo: string) => {
      const respuesta = await pedirApi(`/api/v1/auth/recuperar`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ correo }),
      })
      if (respuesta.status !== 202) throw await armarError(respuesta)
    },
  })
}
