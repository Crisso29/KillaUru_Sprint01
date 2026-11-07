// T073 — PaginaResumen: 6 rótulos, «—», selector días, paneles técnicos, últimos cambios
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaResumen } from './PaginaResumen'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

describe('PaginaResumen', () => {
  it('muestra los 6 rótulos de indicadores', async () => {
    renderizar(<PaginaResumen />)
    await waitFor(() => {
      expect(screen.getByText('Eventos válidos (ayer)')).toBeTruthy()
      expect(screen.getByText('Conductores activos')).toBeTruthy()
      expect(screen.getByText('Dispositivos 24 h')).toBeTruthy()
      expect(screen.getByText('Cobertura')).toBeTruthy()
      expect(screen.getByText('Confirmación')).toBeTruthy()
      expect(screen.getByText('Reaparición')).toBeTruthy()
    })
  })

  it('valor null aparece como «—»', async () => {
    server.use(
      http.get(`${BASE}/api/v1/dashboard/indicadores`, () =>
        HttpResponse.json({
          ultimoDiaCerrado: null,
          calculadoUtc: '2026-11-07T05:30:00Z',
          urlPanelesTecnicos: 'https://killauru.grafana.net/d/api',
          indicadores: {
            eventosDia: { valor: null, comparacion: null, variacion: null, unidad: 'eventos', serie: [] },
            conductoresActivos: { valor: 148, comparacion: 136, variacion: 12, unidad: 'cuentas', serie: [] },
            dispositivosDia: { valor: 203, comparacion: 207, variacion: -4, unidad: 'dispositivos', serie: [] },
            coberturaPct: { valor: 71.2, comparacion: 67.2, variacion: 4, unidad: '%', serie: [] },
            confirmacionPct: { valor: 64, comparacion: 63, variacion: 1, unidad: '%', serie: [] },
            reaparicionPct: { valor: 9, comparacion: 7, variacion: 2, unidad: '%', serie: [] },
          },
          eventosPorDia: [],
        }),
      ),
    )
    renderizar(<PaginaResumen />)
    await waitFor(() => {
      expect(screen.getAllByText('—').length).toBeGreaterThanOrEqual(1)
    })
  })

  it('selector 7/14/30 envía el parámetro dias', async () => {
    let diasCapturado = ''
    server.use(
      http.get(`${BASE}/api/v1/dashboard/indicadores`, ({ request }) => {
        diasCapturado = new URL(request.url).searchParams.get('dias') ?? ''
        return HttpResponse.json({
          ultimoDiaCerrado: '2026-11-06',
          calculadoUtc: '2026-11-07T05:30:00Z',
          urlPanelesTecnicos: 'https://killauru.grafana.net/d/api',
          indicadores: {
            eventosDia: { valor: 100, comparacion: 90, variacion: 11, unidad: 'eventos', serie: [] },
            conductoresActivos: { valor: 50, comparacion: 45, variacion: 5, unidad: 'cuentas', serie: [] },
            dispositivosDia: { valor: 80, comparacion: 85, variacion: -5, unidad: 'dispositivos', serie: [] },
            coberturaPct: { valor: 70, comparacion: 65, variacion: 5, unidad: '%', serie: [] },
            confirmacionPct: { valor: 60, comparacion: 58, variacion: 2, unidad: '%', serie: [] },
            reaparicionPct: { valor: 8, comparacion: 6, variacion: 2, unidad: '%', serie: [] },
          },
          eventosPorDia: [],
        })
      }),
    )
    renderizar(<PaginaResumen />)
    const user = userEvent.setup()
    await waitFor(() => expect(screen.getByLabelText('7 días')).toBeTruthy())
    await user.click(screen.getByLabelText('7 días'))
    await waitFor(() => expect(diasCapturado).toBe('7'))
  })

  it('«Paneles técnicos» abre en otra pestaña con rel="noopener noreferrer"', async () => {
    renderizar(<PaginaResumen />)
    await waitFor(() => {
      const enlace = screen.getByRole('link', { name: /paneles técnicos/i })
      expect(enlace.getAttribute('target')).toBe('_blank')
      expect(enlace.getAttribute('rel')).toBe('noopener noreferrer')
    })
  })

  it('«Estado del servicio» no muestra cifras de disponibilidad', async () => {
    renderizar(<PaginaResumen />)
    await waitFor(() => {
      expect(screen.queryByText(/99,9/)).toBeNull()
      expect(screen.queryByText(/uptime/i)).toBeNull()
    })
  })

  it('últimos cambios muestra «sistema» cuando actorId es null', async () => {
    renderizar(<PaginaResumen />)
    await waitFor(() => {
      expect(screen.getAllByText('sistema').length).toBeGreaterThanOrEqual(1)
    })
  })

  it('error en indicadores no rompe el resto de la pantalla', async () => {
    server.use(
      http.get(`${BASE}/api/v1/dashboard/indicadores`, () =>
        HttpResponse.json({ codigo: 'error_test' }, { status: 500 }),
      ),
    )
    renderizar(<PaginaResumen />)
    await waitFor(() => {
      expect(screen.getByRole('alert')).toBeTruthy()
    })
  })
})
