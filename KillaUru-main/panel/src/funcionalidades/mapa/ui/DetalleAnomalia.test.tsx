import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { DetalleAnomalia } from './DetalleAnomalia'

// T041 — DetalleAnomalia: 17 campos, venceUtc null, demo, historial, 404

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const ID = 'a1b2c3d4-0000-0000-0000-000000000001'
const ACTOR_ID = 'b2c3d4e5-0000-0000-0000-000000000001'

const DETALLE_BASE = {
  id: ID,
  tipo: 'Bache',
  severidadMostrada: 3,
  estado: 'Activa',
  persistencia: 'Episodica',
  latitud: -13.2,
  longitud: -73.5,
  kmVia: 287.4,
  diasDistintos15d: 5,
  reportes: 12,
  ultimaDeteccionUtc: '2026-11-05T08:00:00Z',
  venceUtc: '2026-11-15T08:00:00Z',
  visibleConductores: true,
  porRevisar: false,
  vehiculosRevision: 0,
  reportesPorDia: [
    { dia: '2026-11-01', cantidad: 2 },
    { dia: '2026-11-05', cantidad: 4 },
  ],
  historial: [
    { desde: null, hacia: 'Activa', ocurridaUtc: '2026-10-01T10:00:00Z', actorId: null },
    { desde: 'Activa', hacia: 'PendienteConfirmacion', ocurridaUtc: '2026-10-15T10:00:00Z', actorId: ACTOR_ID },
  ],
}

describe('DetalleAnomalia', () => {
  it('muestra los 17 campos requeridos', async () => {
    renderizar(<DetalleAnomalia id={ID} />)
    await waitFor(() => {
      expect(screen.getByText(/Bache/)).toBeTruthy()
      expect(screen.getByText(/287,4/)).toBeTruthy()
      expect(screen.getByText(/Activa/)).toBeTruthy()
      expect(screen.getByText(/12/)).toBeTruthy()
      expect(screen.getByText(/Episodica/i)).toBeTruthy()
      expect(screen.getAllByText(/5/).length).toBeGreaterThan(0)
    })
  })

  it('venceUtc null → muestra «—»', async () => {
    server.use(
      http.get(`${BASE}/api/v1/anomalies/${ID}`, () =>
        HttpResponse.json({ ...DETALLE_BASE, venceUtc: null }),
      ),
    )
    renderizar(<DetalleAnomalia id={ID} />)
    await waitFor(() => {
      expect(screen.getAllByText('—').length).toBeGreaterThan(0)
    })
  })

  it('km 287,4 → dentro del tramo demo', async () => {
    renderizar(<DetalleAnomalia id={ID} />)
    await waitFor(() => {
      expect(screen.getByText(/tramo demo/i)).toBeTruthy()
    })
  })

  it('historial muestra «sistema» para actorId null y correo para UUID', async () => {
    renderizar(<DetalleAnomalia id={ID} />)
    await waitFor(() => {
      expect(screen.getByText(/sistema/)).toBeTruthy()
      expect(screen.getByText(/actor-b2c3d4e5@killauru\.pe/)).toBeTruthy()
    })
  })

  it('solo un ?ids= con los actores no nulos', async () => {
    let idsCapturados = ''
    server.use(
      http.get(`${BASE}/api/v1/usuarios`, ({ request }) => {
        idsCapturados = new URL(request.url).searchParams.get('ids') ?? ''
        return HttpResponse.json({
          elementos: [{ id: ACTOR_ID, correo: 'admin@killauru.pe', rol: 'Administrador', estado: 'Activa', correoVerificado: true, fechaAltaUtc: '2026-01-01T00:00:00Z', ultimaSesionUtc: null }],
          pagina: 1, tamano: 200, total: 1,
        })
      }),
    )
    renderizar(<DetalleAnomalia id={ID} />)
    await waitFor(() => {
      expect(screen.getByText(/admin@killauru\.pe/)).toBeTruthy()
      expect(idsCapturados).toBe(ACTOR_ID)
      expect(idsCapturados).not.toContain('null')
    })
  })

  it('404 → «Esta anomalía ya no existe.»', async () => {
    server.use(
      http.get(`${BASE}/api/v1/anomalies/no-existe`, () =>
        HttpResponse.json({ codigo: 'no_encontrado' }, { status: 404 }),
      ),
    )
    renderizar(<DetalleAnomalia id="no-existe" />)
    await waitFor(() => {
      expect(screen.getByText(/ya no existe/i)).toBeTruthy()
    })
  })
})
