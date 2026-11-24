// T023 — piezas del proveedor de sesión: renovar el token, programar la próxima renovación y avisar entre pestañas
import { useCallback, useEffect, useMemo, useRef, type Dispatch } from 'react'
import type { AccionSesion } from './estadoSesion'
import { crearRenovador } from './renovador'
import { crearCanalSesion } from './canalSesion'
import { calcularDesfase, msDeltaRenovacion } from './planificador'
import { pedirApi } from '@/api/http'
import { leerSesionPanel } from './sesionPanel'
import type { Reloj } from '@/compartido/reloj'

/** `POST /auth/renovar` con una sola renovación a la vez; un 401 cierra la sesión en el reductor. */
export function useRenovador(despachar: Dispatch<AccionSesion>, reloj: Reloj) {
  const llamarRenovar = useCallback(async (): Promise<Date> => {
    const respuesta = await pedirApi('/api/v1/auth/renovar', { method: 'POST' }, { renovarAnte401: false })
    if (respuesta.status === 401) {
      despachar({ tipo: 'renovar_401' })
      throw new Error('401')
    }
    const { expiraAccesoUtc, sesionVenceUtc } = leerSesionPanel(await respuesta.json())
    despachar({ tipo: 'renovar_ok', expiraAccesoUtc, sesionVenceUtc, desfaseMs: calcularDesfase(expiraAccesoUtc, reloj.ahora()) })
    return expiraAccesoUtc
  }, [despachar, reloj])
  return useMemo(() => crearRenovador(llamarRenovar).renovarSesion, [llamarRenovar])
}

/**
 * Cada renovación programa la siguiente: el token dura 5 min y la sesión 8 h. Antes solo se renovaba una vez y el
 * panel fallaba a los ~10 min. `detener` corta la cadena al salir: sin sesión no se le pide nada al servidor.
 */
export function useRenovacionPeriodica(renovarSesion: () => Promise<Date>, reloj: Reloj) {
  const temporizador = useRef<ReturnType<typeof setTimeout> | null>(null)
  const detener = useCallback(() => {
    if (temporizador.current) clearTimeout(temporizador.current)
    temporizador.current = null
  }, [])
  const programar = useCallback((vence: Date) => {
    detener()
    const siguiente = () => void renovarSesion().then(programar).catch(() => {})
    temporizador.current = setTimeout(siguiente, msDeltaRenovacion(vence, reloj.ahora()))
  }, [renovarSesion, reloj, detener])
  useEffect(() => detener, [detener])
  return useMemo(() => ({ programar, detener }), [programar, detener])
}

/**
 * «Salir»: corta las renovaciones, cierra la sesión en el servidor y avisa a las demás pestañas para que también
 * salgan (data-model §1). Si es OTRA pestaña la que sale, esta termina igual, sin volver a llamar al servidor.
 */
export function useSalida(renovacion: { detener: () => void }, despachar: Dispatch<AccionSesion>) {
  const terminarAqui = useCallback(() => { renovacion.detener(); despachar({ tipo: 'salir' }) }, [renovacion, despachar])
  const avisarSalida = useCanalEntrePestanas(terminarAqui)
  return useCallback(async () => {
    renovacion.detener()
    await pedirApi('/api/v1/auth/cerrar-sesion', { method: 'POST' }, { renovarAnte401: false })
    avisarSalida()
    terminarAqui()
  }, [renovacion, avisarSalida, terminarAqui])
}

/** BroadcastChannel entre pestañas: `avisarSalida` cierra las demás; `alCerrarOtra` se llama cuando otra pestaña sale. */
export function useCanalEntrePestanas(alCerrarOtra: () => void) {
  const alCerrar = useRef(alCerrarOtra)
  alCerrar.current = alCerrarOtra
  const canal = useRef<ReturnType<typeof crearCanalSesion> | null>(null)
  useEffect(() => {
    canal.current = crearCanalSesion(() => alCerrar.current())
    return () => { canal.current?.destruir(); canal.current = null }
  }, [])
  return useCallback(() => canal.current?.emitirCerrada(), [])
}
