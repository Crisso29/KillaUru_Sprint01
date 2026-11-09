// T054 — DialogoRevision: una decisión por admin; body {confirmados,retirados}; 400 recarga
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { DialogoRevision } from './DialogoRevision'
import type { EstadoRevision } from '@/api/dtos/001-identidad'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

const REVISION: EstadoRevision = {
  ultimaRevisionUtc: '2026-10-01T00:00:00Z',
  venceUtc: '2026-12-01T05:00:00Z',
  administradores: [
    { id: 'adm-1', correo: 'admin1@killauru.pe', rol: 'Administrador', estado: 'Activa', correoVerificado: true, fechaAltaUtc: '2026-01-01T00:00:00Z', ultimaSesionUtc: null },
    { id: 'adm-2', correo: 'admin2@killauru.pe', rol: 'Administrador', estado: 'Activa', correoVerificado: true, fechaAltaUtc: '2026-01-01T00:00:00Z', ultimaSesionUtc: null },
  ],
}

describe('DialogoRevision', () => {
  it('muestra a cada administrador', async () => {
    renderizar(<DialogoRevision revision={REVISION} onCerrar={() => {}} />)
    await waitFor(() => {
      expect(screen.getByText(/admin1@killauru\.pe/)).toBeTruthy()
      expect(screen.getByText(/admin2@killauru\.pe/)).toBeTruthy()
    })
  })

  it('envía confirmados y retirados correctamente', async () => {
    let cuerpo: unknown = null
    server.use(
      http.post(`${BASE}/api/v1/usuarios/revision-privilegios`, async ({ request }) => {
        cuerpo = await request.json()
        return HttpResponse.json(
          { ...REVISION, ultimaRevisionUtc: new Date().toISOString() },
          { status: 201 },
        )
      }),
    )
    renderizar(<DialogoRevision revision={REVISION} onCerrar={() => {}} />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getAllByRole('radio', { name: /confirmar/i }).length).toBeGreaterThan(0))
    const confirmar = screen.getAllByRole('radio', { name: /confirmar/i })
    const retirar = screen.getAllByRole('radio', { name: /retirar/i })
    await user.click(confirmar[0]!)
    await user.click(retirar[1]!)
    await user.click(screen.getByRole('button', { name: /guardar/i }))
    await waitFor(() => {
      expect(cuerpo).toMatchObject({ confirmados: ['adm-1'], retirados: ['adm-2'] })
    })
  })

  it('400 revision_incompleta → muestra mensaje y no cierra', async () => {
    server.use(
      http.post(`${BASE}/api/v1/usuarios/revision-privilegios`, () =>
        HttpResponse.json({ codigo: 'revision_incompleta' }, { status: 400 }),
      ),
    )
    renderizar(<DialogoRevision revision={REVISION} onCerrar={() => {}} />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getAllByRole('radio', { name: /confirmar/i }).length).toBeGreaterThan(0))
    const confirmar = screen.getAllByRole('radio', { name: /confirmar/i })
    await user.click(confirmar[0]!)
    await user.click(confirmar[1]!)
    await user.click(screen.getByRole('button', { name: /guardar/i }))
    await waitFor(() =>
      expect(screen.getByText(/debes tomar una decisión para cada administrador/i)).toBeTruthy(),
    )
    expect(screen.getByRole('dialog')).toBeTruthy()
  })
})
