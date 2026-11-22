import { createContext, useCallback, useEffect, useMemo, useReducer, useRef } from 'react'
import type { EstadoSesion } from './estadoSesion'
import { reducirSesion } from './estadoSesion'
import { crearRenovador } from './renovador'
import { crearCanalSesion } from './canalSesion'
import { msDeltaRenovacion } from './planificador'
import { entorno } from '@/config/entorno'
import { relojReal } from '@/compartido/reloj'
import { leerSesionPanel, type DatosSesion } from './sesionPanel'
import type { Reloj } from '@/compartido/reloj'

interface ContextoSesionVal {
  sesion: EstadoSesion
  cerrarSesion: () => Promise<void>
  notificarLogin: (sesion: DatosSesion) => void
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
    const { expiraAccesoUtc, sesionVenceUtc } = leerSesionPanel(await r.json())
    const desfaseMs = expiraAccesoUtc.getTime() - 5 * 60_000 - reloj.ahora().getTime()
    despachar({ tipo: 'renovar_ok', expiraAccesoUtc, sesionVenceUtc, desfaseMs })
    return expiraAccesoUtc
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
    await fetch(`${entorno.urlApi}/api/v1/auth/cerrar-sesion`, { method: 'POST', credentials: 'include' })
    despachar({ tipo: 'salir' })
  }, [])

  // La hora de fin de sesión es la del servidor (8 h desde el login, contrato 003), no una calculada aquí.
  const notificarLogin = useCallback(({ expiraAccesoUtc, sesionVenceUtc }: DatosSesion) => {
    const desfaseMs = expiraAccesoUtc.getTime() - 5 * 60_000 - reloj.ahora().getTime()
    despachar({ tipo: 'login_ok', expiraAccesoUtc, sesionVenceUtc, desfaseMs })
    programarRenovacion(expiraAccesoUtc)
  }, [reloj, programarRenovacion])

  return (
    <ContextoSesion.Provider value={{ sesion, cerrarSesion, notificarLogin }}>
      {children}
    </ContextoSesion.Provider>
  )
}
