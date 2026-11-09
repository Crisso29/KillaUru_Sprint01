// T065 — PaginaModeracion: pestañas, MantenerDescartada sin diálogo, pendientes solo lectura
import { describe, it, expect } from 'vitest'
import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { server } from '@/../pruebas/msw/servidor'
import { http, HttpResponse } from 'msw'
import { PaginaModeracion } from './PaginaModeracion'

const BASE = import.meta.env.VITE_URL_API ?? 'https://api.killauru.pe'

describe('PaginaModeracion', () => {
  it('muestra las dos pestañas con contador', async () => {
    renderizar(<PaginaModeracion />)
    await waitFor(() => {
      expect(screen.getByRole('tab', { name: /por revisar/i })).toBeTruthy()
      expect(screen.getByRole('tab', { name: /esperan confirmación/i })).toBeTruthy()
    })
    await waitFor(() => {
      const tabPorRevisar = screen.getByRole('tab', { name: /por revisar/i })
      expect(tabPorRevisar.textContent).toMatch(/2/)
    })
  })

  it('pestaña Por revisar — muestra filas con accion Mantener descartada y Reabrir', async () => {
    renderizar(<PaginaModeracion />)
    await waitFor(() =>
      expect(screen.getAllByRole('row').length).toBeGreaterThan(1),
    )
    expect(screen.getAllByRole('button', { name: /mantener descartada/i }).length).toBeGreaterThan(0)
    expect(screen.getAllByRole('button', { name: /reabrir/i }).length).toBeGreaterThan(0)
  })

  it('Mantener descartada — POST sin diálogo', async () => {
    let llamado = false
    server.use(
      http.post(`${BASE}/api/v1/anomalies/:id/moderacion`, async ({ request }) => {
        const body = await request.json() as { accion: string }
        if (body.accion === 'MantenerDescartada') llamado = true
        return HttpResponse.json({ id: 'd1d1d1d1-0000-0000-0000-000000000001' })
      }),
    )
    const user = userEvent.setup()
    renderizar(<PaginaModeracion />)
    await waitFor(() =>
      expect(screen.getAllByRole('button', { name: /mantener descartada/i }).length).toBeGreaterThan(0),
    )
    await user.click(screen.getAllByRole('button', { name: /mantener descartada/i })[0]!)
    await waitFor(() => expect(llamado).toBe(true))
    expect(screen.queryByRole('dialog')).toBeNull()
  })

  it('Reabrir — abre diálogo de confirmación', async () => {
    const user = userEvent.setup()
    renderizar(<PaginaModeracion />)
    await waitFor(() =>
      expect(screen.getAllByRole('button', { name: /reabrir/i }).length).toBeGreaterThan(0),
    )
    await user.click(screen.getAllByRole('button', { name: /reabrir/i })[0]!)
    await waitFor(() => expect(screen.getByRole('dialog')).toBeTruthy())
  })

  it('pestaña Esperan confirmación — solo lectura con Ver en el mapa', async () => {
    const user = userEvent.setup()
    renderizar(<PaginaModeracion />)
    await waitFor(() =>
      expect(screen.getByRole('tab', { name: /esperan confirmación/i })).toBeTruthy(),
    )
    await user.click(screen.getByRole('tab', { name: /esperan confirmación/i }))
    await waitFor(() =>
      expect(screen.getAllByRole('link', { name: /ver en el mapa/i }).length).toBeGreaterThan(0),
    )
    expect(screen.queryByRole('button', { name: /descartar/i })).toBeNull()
  })

  it('lista vacía muestra texto de vacío', async () => {
    server.use(
      http.get(`${BASE}/api/v1/moderacion/por-revisar`, () => HttpResponse.json([])),
    )
    renderizar(<PaginaModeracion />)
    await waitFor(() =>
      expect(
        screen.getByText(/no hay anomalías descartadas que requieran revisión/i),
      ).toBeTruthy(),
    )
  })
})
