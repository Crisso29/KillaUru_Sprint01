// T064 — AccionesModeracion: confirmación, POST, 409 transicion_no_permitida, invalidaciones
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { AccionesModeracion } from './AccionesModeracion'
import type { DetalleAnomalia } from '@/api/generado/003-anomalias'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

const DETALLE_ACTIVA: DetalleAnomalia = {
  id: 'a1b2c3d4-0000-0000-0000-000000000001',
  tipo: 'Bache',
  severidadMostrada: 3,
  estado: 'Activa',
  persistencia: 'Transitoria',
  latitud: -13.5,
  longitud: -74.0,
  kmVia: 290.5,
  diasDistintos15d: 5,
  reportes: 10,
  ultimaDeteccionUtc: '2026-11-06T12:00:00Z',
  venceUtc: null,
  visibleConductores: true,
  porRevisar: false,
  vehiculosRevision: 3,
  reportesPorDia: [],
  historial: [],
}

const DETALLE_REPARADA: DetalleAnomalia = { ...DETALLE_ACTIVA, estado: 'Reparada' }

describe('AccionesModeracion', () => {
  it('desde Activa → botones MarcarReparada y Descartar habilitados', async () => {
    renderizar(<AccionesModeracion detalle={DETALLE_ACTIVA} onModerado={() => {}} />)
    expect(screen.getByRole('button', { name: /marcar.*reparada/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /descartar/i })).toBeTruthy()
  })

  it('desde Reparada → Reabrir habilitado; botones no disponibles con tooltip', async () => {
    renderizar(<AccionesModeracion detalle={DETALLE_REPARADA} onModerado={() => {}} />)
    expect(screen.getByRole('button', { name: /reabrir/i })).toBeTruthy()
    const noDisponibles = screen.getAllByTitle(/no disponible desde reparada/i)
    expect(noDisponibles.length).toBeGreaterThan(0)
  })

  it('clic en MarcarReparada → abre diálogo de confirmación', async () => {
    const user = userEvent.setup()
    renderizar(<AccionesModeracion detalle={DETALLE_ACTIVA} onModerado={() => {}} />)
    await user.click(screen.getByRole('button', { name: /marcar como reparada/i }))
    await waitFor(() => {
      expect(screen.getByRole('dialog')).toBeTruthy()
      expect(screen.getByRole('heading', { name: /marcar.*como reparada/i })).toBeTruthy()
      expect(screen.getByText(/queda en la auditoría/i)).toBeTruthy()
    })
  })

  it('confirmar MarcarReparada → POST /moderacion con accion=MarcarReparada', async () => {
    let body: { accion: string } | null = null
    server.use(
      http.post(`${BASE}/api/v1/anomalies/:id/moderacion`, async ({ request }) => {
        body = await request.json() as { accion: string }
        return HttpResponse.json({ ...DETALLE_ACTIVA, estado: 'Reparada' })
      }),
    )
    const user = userEvent.setup()
    renderizar(<AccionesModeracion detalle={DETALLE_ACTIVA} onModerado={() => {}} />)
    await user.click(screen.getByRole('button', { name: /marcar.*reparada/i }))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())
    await user.click(screen.getByRole('button', { name: /confirmar/i }))
    await waitFor(() => expect(body?.accion).toBe('MarcarReparada'))
  })

  it('409 transicion_no_permitida → muestra aviso y cierra diálogo', async () => {
    server.use(
      http.post(`${BASE}/api/v1/anomalies/:id/moderacion`, () =>
        HttpResponse.json({ codigo: 'transicion_no_permitida' }, { status: 409 }),
      ),
    )
    const user = userEvent.setup()
    renderizar(<AccionesModeracion detalle={DETALLE_ACTIVA} onModerado={() => {}} />)
    await user.click(screen.getByRole('button', { name: /marcar.*reparada/i }))
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())
    await user.click(screen.getByRole('button', { name: /confirmar/i }))
    await waitFor(() =>
      expect(screen.getByText(/ya no se puede.*anomalía cambió/i)).toBeTruthy(),
    )
  })
})
