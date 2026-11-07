import { describe, it, expect, vi, beforeEach } from 'vitest'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { crearCliente } from './clientes'

// T016 — credentials include; 401 token_invalido → renova + reintenta;
//        segundo 401 → sesión terminada; 401 credenciales_invalidas no renueva

const BASE = 'http://test-api'

describe('middleware HTTP', () => {
  let onSesionTerminada: ReturnType<typeof vi.fn>

  beforeEach(() => {
    onSesionTerminada = vi.fn()
  })

  it('toda petición sale con credentials: include y sin Authorization', async () => {
    let cabecerasCapturadas: Headers | null = null
    server.use(
      http.get(`${BASE}/recurso`, ({ request }) => {
        cabecerasCapturadas = request.headers
        return HttpResponse.json({})
      }),
    )
    const cliente = crearCliente({
      urlBase: BASE,
      renovar: async () => new Date(),
      onSesionTerminada,
    })
    await cliente.GET('/recurso' as never)
    expect(cabecerasCapturadas?.get('authorization')).toBeNull()
  })

  it('401 token_invalido → 1 renovación y 1 reintento', async () => {
    let llamadas = 0
    server.use(
      http.get(`${BASE}/privado`, () => {
        llamadas++
        if (llamadas === 1)
          return HttpResponse.json({ codigo: 'token_invalido' }, { status: 401 })
        return HttpResponse.json({ ok: true })
      }),
    )
    const renovar = vi.fn(async () => new Date())
    const cliente = crearCliente({ urlBase: BASE, renovar, onSesionTerminada })
    await cliente.GET('/privado' as never)
    expect(renovar).toHaveBeenCalledTimes(1)
    expect(llamadas).toBe(2)
  })

  it('segundo 401 → sesión terminada', async () => {
    server.use(
      http.get(`${BASE}/siempre401`, () =>
        HttpResponse.json({ codigo: 'token_invalido' }, { status: 401 }),
      ),
    )
    const cliente = crearCliente({
      urlBase: BASE,
      renovar: async () => new Date(),
      onSesionTerminada,
    })
    await expect(cliente.GET('/siempre401' as never)).rejects.toBeDefined()
    expect(onSesionTerminada).toHaveBeenCalledOnce()
  })

  it('401 credenciales_invalidas no renueva', async () => {
    server.use(
      http.post(`${BASE}/login`, () =>
        HttpResponse.json({ codigo: 'credenciales_invalidas' }, { status: 401 }),
      ),
    )
    const renovar = vi.fn()
    const cliente = crearCliente({ urlBase: BASE, renovar, onSesionTerminada })
    await expect(cliente.POST('/login' as never, {})).rejects.toBeDefined()
    expect(renovar).not.toHaveBeenCalled()
  })
})
