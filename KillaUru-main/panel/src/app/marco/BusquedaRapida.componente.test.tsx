import { describe, it, expect } from 'vitest'
import { screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useLocation } from 'react-router'
import { renderizar } from '@/../pruebas/renderizar'
import { BusquedaRapida } from './BusquedaRapida'

// T042 — BusquedaRapida: Ctrl+K enfoca, Enter navega, sin resultados avisa

function PaginaActual() {
  const { pathname, search } = useLocation()
  return <div data-testid="ruta-actual">{pathname}{search}</div>
}

function renderBusqueda() {
  return renderizar(
    <>
      <BusquedaRapida />
      <PaginaActual />
    </>,
  )
}

describe('BusquedaRapida', () => {
  it('Ctrl+K abre el campo de búsqueda', async () => {
    renderBusqueda()
    expect(screen.queryByRole('searchbox')).toBeNull()
    fireEvent.keyDown(document.body, { key: 'k', ctrlKey: true, bubbles: true })
    await waitFor(() => {
      expect(screen.getByRole('searchbox')).toBeTruthy()
    })
  })

  it('Enter con UUID navega a /mapa?anomalia=', async () => {
    renderBusqueda()
    const user = userEvent.setup()
    fireEvent.keyDown(document.body, { key: 'k', ctrlKey: true, bubbles: true })
    await waitFor(() => expect(screen.getByRole('searchbox')).toBeTruthy())
    const UUID = 'a1b2c3d4-0000-0000-0000-000000000001'
    await user.type(screen.getByRole('searchbox'), UUID)
    await user.keyboard('{Enter}')
    await waitFor(() => {
      expect(screen.getByTestId('ruta-actual')).toHaveTextContent('/mapa')
      expect(screen.getByTestId('ruta-actual')).toHaveTextContent(`anomalia=${UUID}`)
    })
  })

  it('Enter con km navega a /mapa?kmVia=', async () => {
    renderBusqueda()
    const user = userEvent.setup()
    fireEvent.keyDown(document.body, { key: 'k', ctrlKey: true, bubbles: true })
    await waitFor(() => expect(screen.getByRole('searchbox')).toBeTruthy())
    await user.type(screen.getByRole('searchbox'), '287,4')
    await user.keyboard('{Enter}')
    await waitFor(() => {
      expect(screen.getByTestId('ruta-actual')).toHaveTextContent('/mapa')
      expect(screen.getByTestId('ruta-actual')).toHaveTextContent('kmVia=287.4')
    })
  })

  it('Enter con correo navega a /usuarios?busqueda=', async () => {
    renderBusqueda()
    const user = userEvent.setup()
    fireEvent.keyDown(document.body, { key: 'k', ctrlKey: true, bubbles: true })
    await waitFor(() => expect(screen.getByRole('searchbox')).toBeTruthy())
    await user.type(screen.getByRole('searchbox'), 'ops@killauru.pe')
    await user.keyboard('{Enter}')
    await waitFor(() => {
      expect(screen.getByTestId('ruta-actual')).toHaveTextContent('/usuarios')
    })
  })

  it('texto suelto → muestra aviso sin resultados', async () => {
    renderBusqueda()
    const user = userEvent.setup()
    fireEvent.keyDown(document.body, { key: 'k', ctrlKey: true, bubbles: true })
    await waitFor(() => expect(screen.getByRole('searchbox')).toBeTruthy())
    await user.type(screen.getByRole('searchbox'), 'texto que no tiene formato')
    await user.keyboard('{Enter}')
    await waitFor(() => {
      expect(screen.getByRole('status')).toHaveTextContent(/sin resultados/i)
    })
  })
})
