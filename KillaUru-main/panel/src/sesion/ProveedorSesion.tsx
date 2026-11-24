import { createContext, useCallback, useEffect, useReducer } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import type { EstadoSesion } from './estadoSesion'
import { reducirSesion } from './estadoSesion'
import { calcularDesfase } from './planificador'
import { configurarSesionHttp } from '@/api/http'
import { relojReal } from '@/compartido/reloj'
import type { DatosSesion } from './sesionPanel'
import type { Reloj } from '@/compartido/reloj'
import { useRenovacionPeriodica, useRenovador, useSalida } from './ganchosSesion'

interface ContextoSesionVal {
  sesion: EstadoSesion
  cerrarSesion: () => Promise<void>
  notificarLogin: (sesion: DatosSesion) => void
}

export const ContextoSesion = createContext<ContextoSesionVal | null>(null)

export function ProveedorSesion({ children, reloj = relojReal }: { children: React.ReactNode; reloj?: Reloj }) {
  const [sesion, despachar] = useReducer(reducirSesion, { tipo: 'desconocida' } as EstadoSesion)
  const renovarSesion = useRenovador(despachar, reloj)
  const renovacion = useRenovacionPeriodica(renovarSesion, reloj)
  const cerrarSesion = useSalida(renovacion, despachar)
  useSesionHttp(renovarSesion, () => despachar({ tipo: 'renovar_401' }))
  useVaciarCacheSinSesion(sesion)
  // Al abrir el panel: si la cookie de sesión sigue viva, se renueva y empieza la cadena de renovaciones
  useEffect(() => { void renovarSesion().then(renovacion.programar).catch(() => {}) }, [renovarSesion, renovacion])
  // La hora de fin de sesión es la del servidor (8 h desde el login, contrato 003), no una calculada aquí.
  const notificarLogin = useCallback(({ expiraAccesoUtc, sesionVenceUtc }: DatosSesion) => {
    despachar({ tipo: 'login_ok', expiraAccesoUtc, sesionVenceUtc, desfaseMs: calcularDesfase(expiraAccesoUtc, reloj.ahora()) })
    renovacion.programar(expiraAccesoUtc)
  }, [reloj, renovacion])
  return <ContextoSesion.Provider value={{ sesion, cerrarSesion, notificarLogin }}>{children}</ContextoSesion.Provider>
}

/**
 * Sin sesión (salir, renovación rechazada u otra pestaña que salió) se vacía la caché de datos: lo que vio este
 * administrador no queda en memoria para quien entre después en la misma pestaña (data-model §1).
 */
function useVaciarCacheSinSesion(sesion: EstadoSesion) {
  const cache = useQueryClient()
  useEffect(() => { if (sesion.tipo === 'anonima') cache.clear() }, [sesion.tipo, cache])
}

/** Las llamadas que reciben un 401 por token vencido renuevan con este mismo renovador (una sola renovación a la vez). */
function useSesionHttp(renovar: () => Promise<Date>, onSesionTerminada: () => void) {
  useEffect(() => {
    configurarSesionHttp({ renovar, onSesionTerminada })
    return () => configurarSesionHttp(null)
  // onSesionTerminada solo despacha al reductor (estable); reconfigurar por cada render no aporta nada
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [renovar])
}
