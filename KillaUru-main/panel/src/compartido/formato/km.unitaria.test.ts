import { describe, it, expect } from 'vitest'
import { formatearKm } from './km'

// T014 — formato de km en el corredor

describe('formatearKm', () => {
  it('entero → sin decimales', () => {
    // Arrange y Act
    const resultado = formatearKm(268)

    // Assert
    expect(resultado).toBe('268,0')
  })
  it('con decimal', () => {
    // Arrange y Act
    const resultado = formatearKm(287.4)

    // Assert
    expect(resultado).toBe('287,4')
  })
  it('km 0', () => {
    // Arrange y Act
    const resultado = formatearKm(0)

    // Assert
    expect(resultado).toBe('0,0')
  })
  it('fin corredor', () => {
    // Arrange y Act
    const resultado = formatearKm(335.073)

    // Assert
    expect(resultado).toBe('335,1')
  })
})
