// FR-007, data-model §1 — al perder la sesión, el login explica por qué: «Tu sesión terminó. Vuelve a entrar.»
import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router'
import { ContextoSesion } from '@/sesion/ProveedorSesion'
import { PaginaLogin } from './PaginaLogin'

const AVISO = 'Tu sesión terminó. Vuelve a entrar.'

function abrirLogin(direccion: string) {
  const sesion = { sesion: { tipo: 'anonima' as const }, cerrarSesion: vi.fn(), notificarLogin: vi.fn() }
  render(
    <QueryClientProvider client={new QueryClient()}>
      <ContextoSesion.Provider value={sesion}>
        <MemoryRouter initialEntries={[direccion]}><PaginaLogin /></MemoryRouter>
      </ContextoSesion.Provider>
    </QueryClientProvider>,
  )
}

describe('PaginaLogin', () => {
  it('llegando por una sesión terminada (?motivo=sesion) muestra el aviso', () => {
    // Arrange y Act
    abrirLogin('/login?motivo=sesion')

    // Assert
    expect(screen.getByRole('status')).toHaveTextContent(AVISO)
  })

  it('al entrar normalmente no muestra ningún aviso', () => {
    // Arrange y Act
    abrirLogin('/login')

    // Assert
    expect(screen.queryByText(AVISO)).not.toBeInTheDocument()
  })
})
