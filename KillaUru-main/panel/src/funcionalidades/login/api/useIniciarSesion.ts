// T031 — mutación para iniciar sesión (US1)
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { armarError } from '@/api/problema'
import { entorno } from '@/config/entorno'

interface CuerpoLogin {
  correo: string
  contrasena: string
  turnstileToken: string
}

interface Opciones {
  onExito?: (expiraAccesoUtc: Date) => void
}

export function useIniciarSesion({ onExito }: Opciones = {}) {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async (cuerpo: CuerpoLogin) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/auth/iniciar`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...cuerpo, canal: 'panel' }),
      })
      if (!r.ok) throw await armarError(r)
      const data = await r.json() as { acceso_vence_utc: string }
      return new Date(data.acceso_vence_utc)
    },
    onSuccess: (expiraAccesoUtc) => {
      onExito?.(expiraAccesoUtc)
      void navigate('/resumen')
    },
  })
}
