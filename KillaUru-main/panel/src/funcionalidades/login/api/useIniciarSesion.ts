// T031 — mutación para iniciar sesión (US1)
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { armarError } from '@/api/problema'
import { entorno } from '@/config/entorno'
import { leerSesionPanel, type DatosSesion } from '@/sesion/sesionPanel'

interface CuerpoLogin {
  correo: string
  contrasena: string
  turnstileToken: string
}

interface Opciones {
  onExito?: (sesion: DatosSesion) => void
}

export function useIniciarSesion({ onExito }: Opciones = {}) {
  const navigate = useNavigate()

  return useMutation({
    mutationFn: async (cuerpo: CuerpoLogin) => {
      const r = await fetch(`${entorno.urlApi}/api/v1/auth/inicio-sesion`, {
        method: 'POST',
        credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...cuerpo, canal: 'panel' }),
      })
      if (!r.ok) throw await armarError(r)
      return leerSesionPanel(await r.json())
    },
    onSuccess: (sesion) => {
      onExito?.(sesion)
      void navigate('/resumen')
    },
  })
}
