// T031 — mutación para solicitar recuperación de contraseña (US1, FR-013)
import { useMutation } from '@tanstack/react-query'
import { armarError } from '@/api/problema'
import { entorno } from '@/config/entorno'

export function useRecuperar() {
  return useMutation({
    mutationFn: async (correo: string) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/auth/recuperar`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ correo }),
      })
      if (r.status !== 202) throw await armarError(r)
    },
  })
}
