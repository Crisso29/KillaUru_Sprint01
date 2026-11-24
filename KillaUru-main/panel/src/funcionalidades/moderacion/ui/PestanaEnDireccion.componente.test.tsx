import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { PaginaModeracion } from './PaginaModeracion'

// La pestaña vive en la dirección: «Requiere atención» del Resumen enlaza a ?pestana=pendientes. Patrón AAA.
vi.mock('../api/useListasModeracion', () => ({
  usePorRevisar: () => ({ data: [], isLoading: false, error: null }),
  usePendientes: () => ({ data: [], isLoading: false, error: null }),
}))
vi.mock('../api/useModerar', () => ({ useModerar: () => ({ mutateAsync: vi.fn(), isPending: false }) }))

const abrir = (direccion: string) =>
  render(<MemoryRouter initialEntries={[direccion]}><PaginaModeracion /></MemoryRouter>)

describe('PaginaModeracion: pestaña desde la dirección', () => {
  it('?pestana=pendientes abre «Esperan confirmación»', () => {
    // Arrange y Act
    abrir('/moderacion?pestana=pendientes')
    // Assert
    expect(screen.getByRole('tab', { name: /Esperan confirmación/ }).getAttribute('aria-selected')).toBe('true')
  })

  it('sin parámetro abre «Por revisar»', () => {
    // Arrange y Act
    abrir('/moderacion')
    // Assert
    expect(screen.getByRole('tab', { name: /Por revisar/ }).getAttribute('aria-selected')).toBe('true')
  })
})
