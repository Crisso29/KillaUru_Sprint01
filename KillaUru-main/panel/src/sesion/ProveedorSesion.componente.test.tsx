// FR-007, R-006 — la sesión del panel se renueva antes de CADA vencimiento del token de acceso (5 min), no solo la
// primera vez. Con la renovación única, quien dejaba el panel abierto más de ~10 min recibía errores en toda pantalla.
import { act, render } from '@testing-library/react'
import { useContext } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { ContextoSesion, ProveedorSesion } from './ProveedorSesion'

const MINUTO = 60_000

/** Lo que responde `POST /auth/renovar` con canal panel: el token vence en 5 min y la sesión en 8 h. */
function respuestaRenovar(): Response {
  const ahora = Date.now()   // con vi.useFakeTimers, Date.now() ES el reloj falso de la prueba
  const cuerpo = { expiraAccesoUtc: new Date(ahora + 5 * MINUTO).toISOString(),
    sesionVenceUtc: new Date(ahora + 8 * 60 * MINUTO).toISOString(), rol: 'Administrador' }
  return new Response(JSON.stringify(cuerpo), { status: 200, headers: { 'Content-Type': 'application/json' } })
}

/** Guarda en `salida.cerrar` la función de cerrar sesión del proveedor, como la usa el botón «Salir». */
function CapturarSalir({ salida }: { salida: { cerrar?: () => Promise<void> } }) {
  salida.cerrar = useContext(ContextoSesion)?.cerrarSesion
  return null
}

/** Abre el panel con el reloj falso y un servidor que siempre renueva; devuelve las rutas pedidas y cómo salir. */
function abrirPanel() {
  vi.useFakeTimers({ now: new Date('2026-11-23T10:00:00Z') })
  const rutas: string[] = []
  vi.stubGlobal('fetch', vi.fn(async (url: string) => { rutas.push(url); return respuestaRenovar() }))
  const salida: { cerrar?: () => Promise<void> } = {}
  const cache = new QueryClient()
  cache.setQueryData(['usuarios'], [{ correo: 'otro.admin@killauru.pe' }]) // datos que dejó la sesión anterior
  render(<QueryClientProvider client={cache}><ProveedorSesion><CapturarSalir salida={salida} /></ProveedorSesion></QueryClientProvider>)
  return { rutas, cache, salir: () => act(() => salida.cerrar!()) }
}

describe('ProveedorSesion', () => {
  afterEach(() => {
    vi.useRealTimers()
    vi.unstubAllGlobals()
  })

  it('renueva antes de cada vencimiento del token, no solo la primera vez', async () => {
    // Arrange — reloj falso: 15 minutos pasan en milisegundos
    vi.useFakeTimers({ now: new Date('2026-11-23T10:00:00Z') })
    const rutas: string[] = []
    vi.stubGlobal('fetch', vi.fn(async (url: string) => { rutas.push(url); return respuestaRenovar() }))
    render(<QueryClientProvider client={new QueryClient()}><ProveedorSesion><span>panel</span></ProveedorSesion></QueryClientProvider>)

    // Act
    await vi.advanceTimersByTimeAsync(15 * MINUTO)

    // Assert — al abrir y luego un minuto antes de cada vencimiento: minutos 0, 4, 8 y 12
    expect(rutas.filter(ruta => ruta.endsWith('/api/v1/auth/renovar'))).toHaveLength(4)
  })

  it('al salir avisa a las demás pestañas para que también cierren (data-model §1)', async () => {
    // Arrange — otra pestaña escucha el mismo canal
    const panel = abrirPanel()
    const otraPestana = new BroadcastChannel('ku-sesion')
    const recibidos: unknown[] = []
    otraPestana.onmessage = (evento: MessageEvent) => recibidos.push(evento.data)

    // Act
    await panel.salir()
    await vi.waitFor(() => expect(recibidos).toHaveLength(1), { timeout: 1000, interval: 10 })

    // Assert
    expect(recibidos).toEqual(['cerrada'])
    otraPestana.close()
  })

  it('al salir deja de renovar: no hay pedidos al servidor sin sesión', async () => {
    // Arrange
    const panel = abrirPanel()
    await vi.advanceTimersByTimeAsync(MINUTO)

    // Act
    await panel.salir()
    await vi.advanceTimersByTimeAsync(15 * MINUTO)

    // Assert — solo la renovación al abrir; ninguna después de salir
    expect(panel.rutas.filter(ruta => ruta.endsWith('/api/v1/auth/renovar'))).toHaveLength(1)
  })

  it('al salir vacía la caché de datos: lo de esta sesión no queda en memoria para la siguiente', async () => {
    // Arrange
    const panel = abrirPanel()

    // Act
    await panel.salir()

    // Assert
    expect(panel.cache.getQueryData(['usuarios'])).toBeUndefined()
  })

  it('si la renovación es rechazada (sesión revocada) también vacía la caché', async () => {
    // Arrange — el servidor ya no reconoce la sesión
    vi.useFakeTimers({ now: new Date('2026-11-23T10:00:00Z') })
    vi.stubGlobal('fetch', vi.fn(async () => new Response(JSON.stringify({ codigo: 'sesion_invalida' }), { status: 401 })))
    const cache = new QueryClient()
    cache.setQueryData(['usuarios'], [{ correo: 'otro.admin@killauru.pe' }])

    // Act
    render(<QueryClientProvider client={cache}><ProveedorSesion><span>panel</span></ProveedorSesion></QueryClientProvider>)
    await act(() => vi.advanceTimersByTimeAsync(MINUTO)) // act: React aplica los efectos del cambio de sesión

    // Assert
    expect(cache.getQueryData(['usuarios'])).toBeUndefined()
  })
})
