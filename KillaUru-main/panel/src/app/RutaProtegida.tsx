import { Navigate, useLocation } from 'react-router'
import { useSesion } from '@/sesion/useSesion'

interface Props { children: React.ReactNode }

export function RutaProtegida({ children }: Props) {
  const { sesion } = useSesion()
  const ubicacion = useLocation()

  if (sesion.tipo === 'desconocida') {
    return <div aria-busy="true" aria-label="Verificando sesión…" style={{ height: '100dvh' }} />
  }
  if (sesion.tipo === 'anonima') {
    const motivo = sesion.motivo === 'sesion' ? '?motivo=sesion' : ''
    return <Navigate to={`/login${motivo}`} state={{ desde: ubicacion.pathname }} replace />
  }
  return <>{children}</>
}
