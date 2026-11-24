// FR-014 — los enlaces de los correos (`/cuenta/…#token=`) abren su página y, al guardar, el mensaje de su propósito
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router'
import { ContextoSesion } from '@/sesion/ProveedorSesion'
import { Rutas } from './rutas'

/** El panel tal como lo abre un enlace del correo: sin sesión y en la dirección del enlace. */
function abrirEnlace(direccion: string) {
  window.location.hash = '#token=abc123' // el token llega en el fragmento, que MemoryRouter no lee
  const sesion = { sesion: { tipo: 'anonima' as const }, cerrarSesion: vi.fn(), notificarLogin: vi.fn() }
  render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <ContextoSesion.Provider value={sesion}>
        <MemoryRouter initialEntries={[direccion]}><Rutas /></MemoryRouter>
      </ContextoSesion.Provider>
    </QueryClientProvider>,
  )
}

describe('Páginas públicas de los enlaces del correo', () => {
  it('/cuenta/verificar verifica el correo', async () => {
    // Arrange y Act
    abrirEnlace('/cuenta/verificar')

    // Assert
    expect(await screen.findByText('Listo, tu correo está verificado.')).toBeInTheDocument()
  })

  it.each([
    ['/cuenta/restablecer', 'Tu contraseña cambió. Cerramos todas tus sesiones: vuelve a entrar.'],
    ['/cuenta/invitacion', 'Tu cuenta está lista. Ya puedes iniciar sesión.'],
  ])('%s fija la contraseña y confirma con el mensaje de su propósito', async (direccion, mensaje) => {
    // Arrange
    abrirEnlace(direccion)
    const user = userEvent.setup()
    await user.type(await screen.findByLabelText('Nueva contraseña'), 'ClaveSegura123')
    await user.type(screen.getByLabelText('Confirmar contraseña'), 'ClaveSegura123')

    // Act
    await user.click(screen.getByRole('button', { name: 'Guardar contraseña' }))

    // Assert
    expect(await screen.findByText(mensaje)).toBeInTheDocument()
  })
})
