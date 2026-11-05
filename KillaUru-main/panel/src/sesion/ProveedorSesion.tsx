import { createContext, useCallback, useEffect, useReducer, useRef } from 'react'
import type { EstadoSesion } from './estadoSesion'
import { reducirSesion } from './estadoSesion'
import { crearRenovador } from './renovador'
import { crearCanalSesion } from './canalSesion'
import { msDeltaRenovacion } from './planificador'
import { entorno } from '@/config/entorno'

interface ContextoSesionVal {
  sesion: EstadoSesion
  cerrarSesion: () => Promise<void>
}

export const ContextoSesion = createContext<ContextoSesionVal | null>(null)

export function ProveedorSesion({ children }: { children: React.ReactNode }) {
  const [sesion, despachar] = useReducer(reducirSesion, { tipo: 'desconocida' } as EstadoSesion)
  const timerId = useRef<ReturnType<typeof setTimeout> | null>(null)

  const llamarRenovar = useCallback(async (): Promise<Date> => {
    const r = await fetch(`${entorno.urlApi}/api/v1/auth/renovar`, {
      method: 'POST', credentials: 'include',
    })
    if (r.status === 401) {
      despachar({ tipo: 'renovar_401' })
      throw new Error('401')
    }
    const body = await r.json() as { acceso_vence_utc: string; sesion_vence_utc?: string }
    const expira = new Date(body.acceso_vence_utc)
    const sesionVence = body.sesion_vence_utc ? new Date(body.sesion_vence_utc) : new Date(Date.now() + 8 * 3600_000)
    const desfaseMs = expira.getTime() - 5 * 60_000 - Date.now()
    despachar({ tipo: 'renovar_ok', expiraAccesoUtc: expira, sesionVenceUtc: sesionVence, desfaseMs })
    return expira
  }, [])

  const { renovarSesion } = crearRenovador(llamarRenovar)

  const programarRenovacion = useCallback((expira: Date) => {
    if (timerId.current) clearTimeout(timerId.current)
    const ms = msDeltaRenovacion(expira, new Date())
    timerId.current = setTimeout(() => void renovarSesion(), ms)
  }, [renovarSesion])

  useEffect(() => {
    void renovarSesion().then(programarRenovacion).catch(() => {})
    const { destruir } = crearCanalSesion(() => despachar({ tipo: 'salir' }))
    return () => { destruir(); if (timerId.current) clearTimeout(timerId.current) }
  }, [])

  const cerrarSesion = useCallback(async () => {
    await fetch(`${entorno.urlApi}/api/v1/auth/cerrar`, { method: 'POST', credentials: 'include' })
    despachar({ tipo: 'salir' })
  }, [])

  return (
    <ContextoSesion.Provider value={{ sesion, cerrarSesion }}>
      {children}
    </ContextoSesion.Provider>
  )
}
