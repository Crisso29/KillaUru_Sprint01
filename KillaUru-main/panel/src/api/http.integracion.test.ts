// T022, FR-006, FR-007 — todas las llamadas del panel salen por `pedirApi`: con la cookie de sesión, sin cabecera
// Authorization, y ante un 401 `token_invalido` renuevan una sola vez y reintentan. Antes cada hook llamaba a fetch
// directo y nadie renovaba ante un 401 (el `crearCliente` que lo hacía no lo usaba nadie).
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { http, HttpResponse } from 'msw'
import { server } from '@/../pruebas/msw/servidor'
import { entorno } from '@/config/entorno'
import { configurarSesionHttp, pedirApi, pedirJson } from './http'

const enApi = (ruta: string) => `${entorno.urlApi}${ruta}`

describe('pedirApi', () => {
  const renovar = vi.fn(async () => new Date())
  const onSesionTerminada = vi.fn()

  beforeEach(() => configurarSesionHttp({ renovar, onSesionTerminada }))
  afterEach(() => { vi.clearAllMocks(); configurarSesionHttp(null) })

  it('sale con la cookie de sesión y sin cabecera Authorization', async () => {
    // Arrange
    let autorizacion: string | null = 'sin llamar'
    server.use(http.get(enApi('/api/v1/recurso'), ({ request }) => {
      autorizacion = request.headers.get('authorization')
      return HttpResponse.json({})
    }))

    // Act
    await pedirApi('/api/v1/recurso')

    // Assert — FR-006: el token vive en la cookie HttpOnly, nunca en JavaScript
    expect(autorizacion).toBeNull()
  })

  it('401 token_invalido → renueva una vez y reintenta', async () => {
    // Arrange
    let llamadas = 0
    server.use(http.get(enApi('/api/v1/privado'), () => (++llamadas === 1
      ? HttpResponse.json({ codigo: 'token_invalido' }, { status: 401 })
      : HttpResponse.json({ ok: true }))))

    // Act
    const respuesta = await pedirApi('/api/v1/privado')

    // Assert
    expect([respuesta.status, llamadas, renovar.mock.calls.length]).toEqual([200, 2, 1])
  })

  it('si el reintento también da 401, termina la sesión', async () => {
    // Arrange
    server.use(http.get(enApi('/api/v1/siempre401'), () => HttpResponse.json({ codigo: 'token_invalido' }, { status: 401 })))

    // Act
    const respuesta = await pedirApi('/api/v1/siempre401')

    // Assert
    expect(respuesta.status).toBe(401)
    expect(onSesionTerminada).toHaveBeenCalledOnce()
  })

  it.each(['credenciales_invalidas', 'cuenta_bloqueada', 'solo_administradores'])(
    '401 %s no renueva: es la respuesta del login, no un token vencido', async (codigo) => {
      // Arrange
      server.use(http.post(enApi('/api/v1/auth/inicio-sesion'), () => HttpResponse.json({ codigo }, { status: 401 })))

      // Act
      const respuesta = await pedirApi('/api/v1/auth/inicio-sesion', { method: 'POST' })

      // Assert
      expect(respuesta.status).toBe(401)
      expect(renovar).not.toHaveBeenCalled()
    })

  it('con renovarAnte401: false no renueva (la propia renovación no puede renovarse)', async () => {
    // Arrange
    server.use(http.post(enApi('/api/v1/auth/renovar'), () => HttpResponse.json({ codigo: 'sesion_invalida' }, { status: 401 })))

    // Act
    const respuesta = await pedirApi('/api/v1/auth/renovar', { method: 'POST' }, { renovarAnte401: false })

    // Assert
    expect(respuesta.status).toBe(401)
    expect(renovar).not.toHaveBeenCalled()
  })

  it('pedirJson lanza el ErrorApi del Problema cuando la respuesta no es 2xx', async () => {
    // Arrange
    server.use(http.get(enApi('/api/v1/prohibido'), () =>
      HttpResponse.json({ codigo: 'sin_permiso', correlacionId: 'c-9' }, { status: 403 })))

    // Act
    const promesa = pedirJson('/api/v1/prohibido')

    // Assert
    await expect(promesa).rejects.toEqual({ status: 403, codigo: 'sin_permiso', correlacionId: 'c-9', bloqueadoHastaUtc: undefined })
  })
})
