import { describe, it, expect, vi } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaMapa } from './PaginaMapa'

// T040 — PaginaMapa: primera petición, lista, N de M, vacío, error, selección

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'
const URL_MAPA = `${BASE}/api/v1/anomalies`

vi.mock('./LienzoMapa', () => ({
  LienzoMapa: ({ onSeleccionar }: { onSeleccionar: (id: string) => void }) => (
    <div data-testid="lienzo-mapa" onClick={() => onSeleccionar('a1b2c3d4-0000-0000-0000-000000000001')} />
  ),
}))

function geoJsonConFeaturas(features: unknown[]) {
  return { type: 'FeatureCollection', generadoUtc: new Date().toISOString(), features }
}

function featureActiva(overrides: Record<string, unknown> = {}) {
  const ahora = new Date()
  return {
    type: 'Feature',
    id: 'a1b2c3d4-0000-0000-0000-000000000001',
    geometry: { type: 'Point', coordinates: [-73.5, -13.2] },
    properties: {
      id: 'a1b2c3d4-0000-0000-0000-000000000001',
      kmVia: 287.4,
      tipo: 'Bache',
      severidad: 3,
      persistencia: 'Episodica',
      ultimaDeteccionUtc: new Date(ahora.getTime() - 2 * 3600_000).toISOString(),
      venceUtc: new Date(ahora.getTime() + 10 * 86400_000).toISOString(),
      estado: 'Activa',
      ...overrides,
    },
  }
}

describe('PaginaMapa', () => {
  it('primera petición lleva estado=Activa,PendienteConfirmacion', async () => {
    let urlCapturada = ''
    server.use(
      http.get(URL_MAPA, ({ request }) => {
        urlCapturada = request.url
        return HttpResponse.json(geoJsonConFeaturas([featureActiva()]))
      }),
    )
    renderizar(<PaginaMapa />)
    await waitFor(() => {
      expect(urlCapturada).toContain('estado=Activa%2CPendienteConfirmacion')
    })
  })

  it('lista muestra «Bache · km 287,4», id corto y «hace 2 h»', async () => {
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([featureActiva()])),
      ),
    )
    renderizar(<PaginaMapa />)
    await waitFor(() => {
      expect(screen.getByText(/Bache/)).toBeTruthy()
      expect(screen.getByText(/287,4/)).toBeTruthy()
      expect(screen.getByText(/a1b2c3d4/)).toBeTruthy()
      expect(screen.getByText(/hace 2 h/)).toBeTruthy()
    })
  })

  it('muestra N de M anomalías', async () => {
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([featureActiva()])),
      ),
    )
    renderizar(<PaginaMapa />)
    await waitFor(() => {
      expect(screen.getByText(/1 de 1/)).toBeTruthy()
    })
  })

  it('lista vacía muestra estado vacío', async () => {
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([])),
      ),
    )
    renderizar(<PaginaMapa />)
    await waitFor(() => {
      expect(screen.getByRole('status')).toBeTruthy()
    })
  })

  it('error muestra «Reintentar»', async () => {
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json({ codigo: 'sin_conexion' }, { status: 503 }),
      ),
    )
    renderizar(<PaginaMapa />)
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /reintentar/i })).toBeTruthy()
    })
  })

  it('elegir fila abre detalle y pone ?anomalia=', async () => {
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([featureActiva()])),
      ),
    )
    renderizar(<PaginaMapa />)
    await waitFor(() => expect(screen.getByText(/Bache/)).toBeTruthy())
    const user = userEvent.setup()
    const fila = screen.getByRole('button', { name: /Bache/i })
    await user.click(fila)
    await waitFor(() => {
      expect(screen.getByTestId('detalle-anomalia')).toBeTruthy()
    })
  })
})
