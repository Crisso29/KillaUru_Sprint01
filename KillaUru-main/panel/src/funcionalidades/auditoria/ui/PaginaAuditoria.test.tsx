// T079 — PaginaAuditoria: filtros, actor, retención
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaAuditoria } from './PaginaAuditoria'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

const DATOS_AUDITORIA = {
  elementos: [
    {
      id: 1001,
      ocurridoUtc: '2026-11-04T10:30:00Z',
      actorId: 'a1a1a1a1-0000-0000-0000-000000000001',
      actorRol: 'Administrador',
      categoria: 'Moderacion',
      accion: 'AnomaliaModerada',
      objetivo: 'b2b2b2b2-0000-0000-0000-000000000001',
      contexto: { accion: 'MarcarReparada' },
    },
    {
      id: 1002,
      ocurridoUtc: '2026-11-04T09:15:00Z',
      actorId: null,
      actorRol: 'Sistema',
      categoria: 'AccesoADatos',
      accion: 'AccesoAdministrativoDatos',
      objetivo: null,
      contexto: {},
    },
  ],
  pagina: 1,
  tamano: 50,
  total: 2,
}

describe('PaginaAuditoria', () => {
  it('muestra «N registros · retención permanente»', async () => {
    renderizar(<PaginaAuditoria />)
    await waitFor(() => {
      expect(screen.getByText(/retención permanente/i)).toBeTruthy()
    })
  })

  it('actorId null se muestra como «sistema»', async () => {
    server.use(
      http.get(`${BASE}/api/v1/auditoria`, () => HttpResponse.json(DATOS_AUDITORIA)),
    )
    renderizar(<PaginaAuditoria />)
    await waitFor(() => {
      expect(screen.getAllByText('sistema').length).toBeGreaterThan(0)
    })
  })

  it('filtro categoría envía el parámetro correcto', async () => {
    let categoriaCapturada = ''
    server.use(
      http.get(`${BASE}/api/v1/auditoria`, ({ request }) => {
        categoriaCapturada = new URL(request.url).searchParams.get('categoria') ?? ''
        return HttpResponse.json(DATOS_AUDITORIA)
      }),
    )
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getByRole('combobox', { name: /categoría/i })).toBeTruthy())
    await user.selectOptions(screen.getByRole('combobox', { name: /categoría/i }), 'Moderacion')
    await waitFor(() => expect(categoriaCapturada).toBe('Moderacion'))
  })

  it('actor por correo lanza 1 ?busqueda= y luego consulta con actorId', async () => {
    let busquedas = 0
    let actorIdCapturado = ''
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        const url = new URL(request.url)
        if (url.searchParams.has('busqueda')) {
          busquedas++
          return HttpResponse.json({
            elementos: [{
              id: 'a1a1a1a1-0000-0000-0000-000000000001',
              correo: 'admin@killauru.pe',
              rol: 'Administrador',
              estado: 'Activa',
              correoVerificado: true,
              fechaAltaUtc: '2026-01-01T00:00:00Z',
              ultimaSesionUtc: null,
            }],
            pagina: 1, tamano: 50, total: 1,
          })
        }
        return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
      }),
      http.get(`${BASE}/api/v1/auditoria`, ({ request }) => {
        actorIdCapturado = new URL(request.url).searchParams.get('actorId') ?? ''
        return HttpResponse.json(DATOS_AUDITORIA)
      }),
    )
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getByRole('searchbox', { name: /actor/i })).toBeTruthy())
    await user.type(screen.getByRole('searchbox', { name: /actor/i }), 'admin@killauru.pe')
    await waitFor(() => {
      expect(busquedas).toBeGreaterThanOrEqual(1)
      expect(actorIdCapturado).toBe('a1a1a1a1-0000-0000-0000-000000000001')
    })
  })

  it('actor correo sin cuenta → aviso', async () => {
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        const url = new URL(request.url)
        if (url.searchParams.has('busqueda')) {
          return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
        }
        return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
      }),
    )
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getByRole('searchbox', { name: /actor/i })).toBeTruthy())
    await user.type(screen.getByRole('searchbox', { name: /actor/i }), 'nadie@killauru.pe')
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeTruthy()
    })
  })

  it('solo 1 ?ids= por página para resolver actores', async () => {
    let idsRequests = 0
    server.use(
      http.get(`${BASE}/api/v1/auditoria`, () => HttpResponse.json(DATOS_AUDITORIA)),
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        const url = new URL(request.url)
        if (url.searchParams.has('ids')) {
          idsRequests++
          return HttpResponse.json({
            elementos: [{
              id: 'a1a1a1a1-0000-0000-0000-000000000001',
              correo: 'admin@killauru.pe',
              rol: 'Administrador',
              estado: 'Activa',
              correoVerificado: true,
              fechaAltaUtc: '2026-01-01T00:00:00Z',
              ultimaSesionUtc: null,
            }],
            pagina: 1, tamano: 50, total: 1,
          })
        }
        return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
      }),
    )
    renderizar(<PaginaAuditoria />)
    await waitFor(() => expect(screen.getByText(/retención permanente/i)).toBeTruthy())
    await waitFor(() => expect(idsRequests).toBe(1))
  })

  it('fila tiene botón aria-expanded y al expandir muestra JSON', async () => {
    server.use(
      http.get(`${BASE}/api/v1/auditoria`, () => HttpResponse.json(DATOS_AUDITORIA)),
    )
    renderizar(<PaginaAuditoria />)
    const user = userEvent.setup()
    await waitFor(() => {
      const botones = screen.getAllByRole('button', { name: /ver contexto/i })
      expect(botones.length).toBeGreaterThan(0)
    })
    const boton = screen.getAllByRole('button', { name: /ver contexto/i })[0]!
    expect(boton.getAttribute('aria-expanded')).toBe('false')
    await user.click(boton)
    await waitFor(() => {
      expect(boton.getAttribute('aria-expanded')).toBe('true')
      expect(screen.getAllByText(/MarcarReparada/).length).toBeGreaterThan(0)
    })
  })
})
