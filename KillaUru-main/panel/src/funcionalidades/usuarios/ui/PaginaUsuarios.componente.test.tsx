// T052 — PaginaUsuarios: lista, contador, filtros, búsqueda, vacío, error
import { describe, it, expect, vi } from 'vitest'
import { screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaUsuarios } from './PaginaUsuarios'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

describe('PaginaUsuarios', () => {
  it('muestra correos de la lista', async () => {
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    await waitFor(() => {
      expect(screen.getByText(/admin@killauru\.pe/)).toBeTruthy()
      expect(screen.getByText(/conductor@killauru\.pe/)).toBeTruthy()
    })
  })

  it('muestra contador «N de total»', async () => {
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    await waitFor(() => {
      expect(screen.getByText(/3 de 3/)).toBeTruthy()
    })
  })

  it('filtro estado tiene opción «InvitacionPendiente»', async () => {
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    await waitFor(() => {
      const sel = screen.getByRole('combobox', { name: /estado/i })
      expect(sel.querySelector('option[value="InvitacionPendiente"]')).toBeTruthy()
    })
  })

  it('cambio de estado envía el param correcto', async () => {
    let estadoCapturado = ''
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        estadoCapturado = new URL(request.url).searchParams.get('estado') ?? ''
        return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
      }),
    )
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getByRole('combobox', { name: /estado/i })).toBeTruthy())
    await user.selectOptions(screen.getByRole('combobox', { name: /estado/i }), 'InvitacionPendiente')
    await waitFor(() => expect(estadoCapturado).toBe('InvitacionPendiente'))
  })

  it('Enter lanza la búsqueda inmediatamente', async () => {
    let busquedaCapturada = ''
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        const b = new URL(request.url).searchParams.get('busqueda')
        if (b) busquedaCapturada = b
        return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
      }),
    )
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getByRole('searchbox')).toBeTruthy())
    await user.type(screen.getByRole('searchbox'), 'jose')
    await user.keyboard('{Enter}')
    await waitFor(() => expect(busquedaCapturada).toBe('jose'))
  })

  it('texto < 3 chars no dispara búsqueda hasta 400ms', async () => {
    vi.useFakeTimers()
    let reqs = 0
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        if (new URL(request.url).searchParams.has('busqueda')) reqs++
        return HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 })
      }),
    )
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'ab' } })
    expect(reqs).toBe(0)
    vi.useRealTimers()
  })

  it('lista vacía muestra estado vacío', async () => {
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, () =>
        HttpResponse.json({ elementos: [], pagina: 1, tamano: 50, total: 0 }),
      ),
    )
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    await waitFor(() => expect(screen.getByRole('status')).toBeTruthy())
  })

  it('error 503 muestra botón Reintentar', async () => {
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, () =>
        HttpResponse.json({ codigo: 'sin_conexion' }, { status: 503 }),
      ),
    )
    renderizar(<PaginaUsuarios correoActual="me@killauru.pe" />)
    await waitFor(() =>
      expect(screen.getByRole('button', { name: /reintentar/i })).toBeTruthy(),
    )
  })
})
