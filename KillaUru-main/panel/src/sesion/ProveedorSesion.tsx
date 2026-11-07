import { createContext, useCallback, useEffect, useMemo, useReducer, useRef } from 'react'
import type { EstadoSesion } from './estadoSesion'
import { reducirSesion } from './estadoSesion'
import { crearRenovador } from './renovador'
import { crearCanalSesion } from './canalSesion'
import { msDeltaRenovacion } from './planificador'
import { entorno } from '@/config/entorno'
import { relojReal } from '@/compartido/reloj'
import type { Reloj } from '@/compartido/reloj'

interface ContextoSesionVal {
  sesion: EstadoSesion
  cerrarSesion: () => Promise<void>
  notificarLogin: (expiraAccesoUtc: Date) => void
}

export const ContextoSesion = createContext<ContextoSesionVal | null>(null)

export function ProveedorSesion({
  children,
  reloj = relojReal,
}: {
  children: React.ReactNode
  reloj?: Reloj
}) {
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
    const ahora = reloj.ahora()
    const sesionVence = body.sesion_vence_utc
      ? new Date(body.sesion_vence_utc)
      : new Date(ahora.getTime() + 8 * 3600_000)
    const desfaseMs = expira.getTime() - 5 * 60_000 - ahora.getTime()
    despachar({ tipo: 'renovar_ok', expiraAccesoUtc: expira, sesionVenceUtc: sesionVence, desfaseMs })
    return expira
  }, [reloj])

  const { renovarSesion } = useMemo(() => crearRenovador(llamarRenovar), [llamarRenovar])

  const programarRenovacion = useCallback((expira: Date) => {
    if (timerId.current) clearTimeout(timerId.current)
    const ms = msDeltaRenovacion(expira, reloj.ahora())
    timerId.current = setTimeout(() => void renovarSesion(), ms)
  }, [renovarSesion, reloj])

  useEffect(() => {
    void renovarSesion().then(programarRenovacion).catch(() => {})
    const { destruir } = crearCanalSesion(() => despachar({ tipo: 'salir' }))
    return () => { destruir(); if (timerId.current) clearTimeout(timerId.current) }
  }, [renovarSesion, programarRenovacion])

  const cerrarSesion = useCallback(async () => {
    await fetch(`${entorno.urlApi}/api/v1/auth/cerrar`, { method: 'POST', credentials: 'include' })
    despachar({ tipo: 'salir' })
  }, [])

  const notificarLogin = useCallback((expiraAccesoUtc: Date) => {
    const ahora = reloj.ahora()
    const desfaseMs = expiraAccesoUtc.getTime() - 5 * 60_000 - ahora.getTime()
    const sesionVenceUtc = new Date(ahora.getTime() + 8 * 3600_000)
    despachar({ tipo: 'login_ok', expiraAccesoUtc, sesionVenceUtc, desfaseMs })
    programarRenovacion(expiraAccesoUtc)
  }, [reloj, programarRenovacion])

  return (
    <ContextoSesion.Provider value={{ sesion, cerrarSesion, notificarLogin }}>
      {children}
    </ContextoSesion.Provider>
  )
}
