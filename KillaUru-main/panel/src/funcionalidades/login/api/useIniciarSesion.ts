// T031 — mutación para iniciar sesión (US1)
import { useMutation } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { armarError } from '@/api/problema'
import { leerSesionPanel, type DatosSesion } from '@/sesion/sesionPanel'
import { pedirApi } from '@/api/http'

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
      const respuesta = await pedirApi(`/api/v1/auth/inicio-sesion`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...cuerpo, canal: 'panel' }),
      })
      if (!respuesta.ok) throw await armarError(respuesta)
      return leerSesionPanel(await respuesta.json())
    },
    onSuccess: (sesion) => {
      onExito?.(sesion)
      void navigate('/resumen')
    },
  })
}
