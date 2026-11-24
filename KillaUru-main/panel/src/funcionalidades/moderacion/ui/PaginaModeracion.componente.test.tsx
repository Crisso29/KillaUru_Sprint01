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
    // Arrange y Act
    renderizar(<PaginaModeracion />)

    // Assert
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
    // Arrange y Act
    renderizar(<PaginaModeracion />)

    // Assert
    await waitFor(() =>
      expect(screen.getAllByRole('row').length).toBeGreaterThan(1),
    )
    expect(screen.getAllByRole('button', { name: /mantener descartada/i }).length).toBeGreaterThan(0)
    expect(screen.getAllByRole('button', { name: /reabrir/i }).length).toBeGreaterThan(0)
  })

  it('Mantener descartada — POST sin diálogo', async () => {
    // Arrange — mantener no cambia nada visible para el conductor: no pide confirmación
    const acciones: string[] = []
    server.use(
      http.post(`${BASE}/api/v1/anomalies/:id/moderacion`, async ({ request }) => {
        acciones.push((await request.json() as { accion: string }).accion)
        return HttpResponse.json({ id: 'd1d1d1d1-0000-0000-0000-000000000001' })
      }),
    )
    const user = userEvent.setup()
    renderizar(<PaginaModeracion />)
    const [mantener] = await screen.findAllByRole('button', { name: /mantener descartada/i })

    // Act
    await user.click(mantener!)

    // Assert
    await waitFor(() => expect(acciones).toEqual(['MantenerDescartada']))
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('Reabrir — abre diálogo de confirmación', async () => {
    // Arrange
    const user = userEvent.setup()
    renderizar(<PaginaModeracion />)
    const [reabrir] = await screen.findAllByRole('button', { name: /reabrir/i })

    // Act
    await user.click(reabrir!)

    // Assert
    expect(await screen.findByRole('dialog')).toBeInTheDocument()
  })

  it('pestaña Esperan confirmación — solo lectura con Ver en el mapa', async () => {
    // Arrange
    const user = userEvent.setup()
    renderizar(<PaginaModeracion />)
    const pestana = await screen.findByRole('tab', { name: /esperan confirmación/i })

    // Act
    await user.click(pestana)

    // Assert
    expect((await screen.findAllByRole('link', { name: /ver en el mapa/i })).length).toBeGreaterThan(0)
    expect(screen.queryByRole('button', { name: /descartar/i })).not.toBeInTheDocument()
  })

  it('lista vacía muestra texto de vacío', async () => {
    // Arrange
    server.use(
      http.get(`${BASE}/api/v1/moderacion/por-revisar`, () => HttpResponse.json([])),
    )

    // Act
    renderizar(<PaginaModeracion />)

    // Assert
    await waitFor(() =>
      expect(
        screen.getByText(/no hay anomalías descartadas que requieran revisión/i),
      ).toBeTruthy(),
    )
  })
})
