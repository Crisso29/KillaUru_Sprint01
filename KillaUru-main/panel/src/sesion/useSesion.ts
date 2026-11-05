// T023 — hook de acceso a la sesión desde cualquier componente
import { useContext } from 'react'
import { ContextoSesion } from './ProveedorSesion'

export function useSesion() {
  const ctx = useContext(ContextoSesion)
  if (!ctx) throw new Error('useSesion debe usarse dentro de ProveedorSesion')
  return ctx
}
