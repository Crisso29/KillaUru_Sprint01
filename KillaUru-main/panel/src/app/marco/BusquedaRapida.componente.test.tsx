import { describe, it, expect } from 'vitest'
import { screen, waitFor, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { renderizar } from '@/../pruebas/renderizar'
import { BusquedaRapida } from './BusquedaRapida'
import { RutaActual, ID_RUTA_ACTUAL } from '@/../pruebas/RutaActual'

// T042 — BusquedaRapida: Ctrl+K enfoca, Enter navega, sin resultados avisa

function renderBusqueda() {
  return renderizar(
    <>
      <BusquedaRapida />
      <RutaActual />
    </>,
  )
}

describe('BusquedaRapida', () => {
  // Prototipo `.search`: el campo está siempre en la barra superior; Ctrl+K lleva el foco a él.
  it('el campo está siempre visible y Ctrl+K lo enfoca', async () => {
    // Arrange
    renderBusqueda()
    const campo = screen.getByRole('searchbox')
    expect(document.activeElement).not.toBe(campo) // condición previa: el foco no empieza ahí

    // Act
    fireEvent.keyDown(document.body, { key: 'k', ctrlKey: true, bubbles: true })

    // Assert
    await waitFor(() => expect(document.activeElement).toBe(campo))
  })

  it.each([
    ['un id completo abre su detalle en el mapa', 'a1b2c3d4-0000-0000-0000-000000000001', '/mapa?anomalia=a1b2c3d4-0000-0000-0000-000000000001'],
    ['un km abre el mapa en ese punto', '287,4', '/mapa?kmVia=287.4'],
    ['un correo abre Usuarios filtrado', 'ops@killauru.pe', '/usuarios?busqueda=ops%40killauru.pe'],
  ])('Enter: %s', async (_caso, texto, rutaEsperada) => {
    // Arrange
    renderBusqueda()
    const user = userEvent.setup()
    await user.type(screen.getByRole('searchbox'), texto)

    // Act
    await user.keyboard('{Enter}')

    // Assert
    await waitFor(() => expect(screen.getByTestId(ID_RUTA_ACTUAL)).toHaveTextContent(rutaEsperada))
  })

  it('texto suelto → muestra aviso sin resultados y no navega', async () => {
    // Arrange
    renderBusqueda()
    const user = userEvent.setup()
    await user.type(screen.getByRole('searchbox'), 'texto que no tiene formato')

    // Act
    await user.keyboard('{Enter}')

    // Assert
    expect(await screen.findByRole('status')).toHaveTextContent(/sin resultados/i)
    expect(screen.getByTestId(ID_RUTA_ACTUAL)).toHaveTextContent(/^\/$/)
  })
})
