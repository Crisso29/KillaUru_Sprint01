// T060 — PaginaVerificar: 204, 400 enlace_invalido, token no se envía dos veces
import { describe, it, expect, afterEach } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaVerificar } from './PaginaVerificar'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

afterEach(() => { window.location.hash = '' })

describe('PaginaVerificar', () => {
  it('204 → muestra confirmación', async () => {
    window.location.hash = '#token=abc-valido'
    renderizar(<PaginaVerificar />)
    await waitFor(() => {
      expect(screen.getByText(/verificado/i)).toBeTruthy()
    })
  })

  it('400 enlace_invalido → muestra error', async () => {
    server.use(
      http.post(`${BASE}/api/v1/auth/verificar-correo`, () =>
        HttpResponse.json({ codigo: 'enlace_invalido' }, { status: 400 }),
      ),
    )
    window.location.hash = '#token=token-vencido'
    renderizar(<PaginaVerificar />)
    await waitFor(() => {
      expect(screen.getByText(/no es válido o ya venció/i)).toBeTruthy()
    })
  })

  it('el token no se envía dos veces', async () => {
    let count = 0
    server.use(
      http.post(`${BASE}/api/v1/auth/verificar-correo`, () => {
        count++
        return new HttpResponse(null, { status: 204 })
      }),
    )
    window.location.hash = '#token=token-unico'
    renderizar(<PaginaVerificar />)
    await waitFor(() => expect(screen.getByText(/verificado/i)).toBeTruthy())
    expect(count).toBe(1)
  })

  it('sin token en el hash → muestra error de enlace', async () => {
    window.location.hash = ''
    renderizar(<PaginaVerificar />)
    await waitFor(() => {
      expect(screen.getByText(/no es válido o ya venció/i)).toBeTruthy()
    })
  })
})
