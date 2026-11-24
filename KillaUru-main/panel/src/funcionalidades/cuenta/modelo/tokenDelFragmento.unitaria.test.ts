// T059 — tokenDelFragmento: lee #token=, llama replaceState, sin token → null
import { describe, it, expect, vi, afterEach } from 'vitest'
import { tokenDelFragmento } from './tokenDelFragmento'

afterEach(() => {
  window.location.hash = ''
  vi.restoreAllMocks()
})

describe('tokenDelFragmento', () => {
  it.each([
    ['#token=abc123', 'abc123'],
    ['#token=', null], // vacío
    ['', null], // sin fragmento
    ['#otro=valor', null],
  ])('fragmento «%s» → %s', (fragmento, esperado) => {
    // Arrange
    window.location.hash = fragmento

    // Act
    const token = tokenDelFragmento()

    // Assert
    expect(token).toBe(esperado)
  })

  it('quita el fragmento de la dirección: el token no queda en el historial ni se copia con el enlace', () => {
    // Arrange
    window.location.hash = '#token=abc123'
    const reemplazar = vi.spyOn(history, 'replaceState')

    // Act
    tokenDelFragmento()

    // Assert
    expect(reemplazar).toHaveBeenCalled()
  })
})
