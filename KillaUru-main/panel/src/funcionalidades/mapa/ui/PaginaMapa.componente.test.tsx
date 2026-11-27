import { describe, it, expect, vi, afterEach } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaMapa } from './PaginaMapa'
import { RutaActual, ID_RUTA_ACTUAL } from '@/../pruebas/RutaActual'

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
  afterEach(() => vi.useRealTimers())
  it('primera petición lleva estado=Activa,PendienteConfirmacion', async () => {
    // Arrange
    let urlCapturada = ''
    server.use(
      http.get(URL_MAPA, ({ request }) => {
        urlCapturada = request.url
        return HttpResponse.json(geoJsonConFeaturas([featureActiva()]))
      }),
    )

    // Act
    renderizar(<PaginaMapa />)

    // Assert
    await waitFor(() => {
      expect(urlCapturada).toContain('estado=Activa%2CPendienteConfirmacion')
    })
  })

  it('lista muestra «Bache · km 287,4», id corto y «hace 2 h»', async () => {
    // Arrange — fijar el reloj en mediodía Lima (UTC−5) para que «hace 2 h» nunca cruce a ayer
    vi.useFakeTimers({ toFake: ['Date'] })
    vi.setSystemTime(new Date('2026-01-15T17:00:00.000Z'))
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([featureActiva()])),
      ),
    )

    // Act
    renderizar(<PaginaMapa />)

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/Bache/)).toBeTruthy()
      expect(screen.getByText(/287,4/)).toBeTruthy()
      expect(screen.getByText(/a1b2c3d4/)).toBeTruthy()
      expect(screen.getByText(/hace 2 h/)).toBeTruthy()
    })
  })

  it('muestra N de M anomalías', async () => {
    // Arrange
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([featureActiva()])),
      ),
    )

    // Act
    renderizar(<PaginaMapa />)

    // Assert
    await waitFor(() => {
      expect(screen.getByText(/1 de 1/)).toBeTruthy()
    })
  })

  it('lista vacía muestra estado vacío', async () => {
    // Arrange
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json(geoJsonConFeaturas([])),
      ),
    )

    // Act
    renderizar(<PaginaMapa />)

    // Assert
    await waitFor(() => {
      expect(screen.getByRole('status')).toBeTruthy()
    })
  })

  it('error muestra «Reintentar»', async () => {
    // Arrange
    server.use(
      http.get(URL_MAPA, () =>
        HttpResponse.json({ codigo: 'sin_conexion' }, { status: 503 }),
      ),
    )

    // Act
    renderizar(<PaginaMapa />)

    // Assert
    await waitFor(() => {
      expect(screen.getByRole('button', { name: /reintentar/i })).toBeTruthy()
    })
  })

  it('elegir fila abre detalle y pone ?anomalia=', async () => {
    // Arrange
    const anomalia = featureActiva()
    server.use(http.get(URL_MAPA, () => HttpResponse.json(geoJsonConFeaturas([anomalia]))))
    renderizar(<><PaginaMapa /><RutaActual /></>)
    const user = userEvent.setup()
    const fila = await screen.findByRole('button', { name: /Bache/i })

    // Act
    await user.click(fila)

    // Assert — el detalle se abre y la dirección lo guarda (un enlace copiado abre la misma anomalía, FR-026)
    expect(await screen.findByTestId('detalle-anomalia')).toBeInTheDocument()
    expect(screen.getByTestId(ID_RUTA_ACTUAL)).toHaveTextContent(`anomalia=${anomalia.properties.id}`)
  })
})
