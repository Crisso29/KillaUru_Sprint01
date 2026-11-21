import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaRecuperar } from './PaginaRecuperar'

// T030 — PaginaRecuperar: 202 siempre igual; 429 con aviso

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const URL_RECUPERAR = `${BASE}/api/v1/auth/recuperar`

async function enviarCorreo(correo: string) {
  const user = userEvent.setup()
  await user.type(screen.getByLabelText(/correo/i), correo)
  await user.click(screen.getByRole('button', { name: /enviar/i }))
}

describe('PaginaRecuperar', () => {
  it('202 → mensaje de confirmación (correo existente)', async () => {
    server.use(
      http.post(URL_RECUPERAR, () => new HttpResponse(null, { status: 202 })),
    )
    renderizar(<PaginaRecuperar />)
    await enviarCorreo('ops@killauru.pe')
    await waitFor(() => {
      expect(screen.getByRole('status')).toHaveTextContent(
        /si.*registrado.*recibirás/i,
      )
    })
  })

  it('202 → mismo texto aunque el correo no exista', async () => {
    server.use(
      http.post(URL_RECUPERAR, () => new HttpResponse(null, { status: 202 })),
    )
    renderizar(<PaginaRecuperar />)
    await enviarCorreo('noexiste@ejemplo.pe')
    await waitFor(() => {
      expect(screen.getByRole('status')).toHaveTextContent(
        /si.*registrado.*recibirás/i,
      )
    })
  })

  it('429 → aviso de límite excedido', async () => {
    server.use(
      http.post(URL_RECUPERAR, () =>
        HttpResponse.json({ codigo: 'limite_excedido' }, { status: 429 }),
      ),
    )
    renderizar(<PaginaRecuperar />)
    await enviarCorreo('ops@killauru.pe')
    await waitFor(() => {
      expect(screen.getByRole('alert')).toHaveTextContent('muchas consultas')
    })
  })
})
