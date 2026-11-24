import { describe, it, expect } from 'vitest'
import { formatearNumero, formatearPorcentaje, formatearDelta, formatearDeltaPts } from './numeros'

// T014 — «287,4» · «8,1 %» · «+12» · «−4» · «+4 pts» · «+1 pt»

describe('formatearNumero', () => {
  it('287.4 → «287,4»', () => {
    // Arrange y Act
    const resultado = formatearNumero(287.4)

    // Assert
    expect(resultado).toBe('287,4')
  })
  it('entero sin decimales', () => {
    // Arrange y Act
    const resultado = formatearNumero(1000)

    // Assert
    expect(resultado).toBe('1000')
  })
  it('cero', () => {
    // Arrange y Act
    const resultado = formatearNumero(0)

    // Assert
    expect(resultado).toBe('0')
  })
})

describe('formatearPorcentaje', () => {
  it('8.1 → «8,1 %»', () => {
    // Arrange y Act
    const resultado = formatearPorcentaje(8.1)

    // Assert
    expect(resultado).toBe('8,1 %')
  })
  it('100 → «100 %»', () => {
    // Arrange y Act
    const resultado = formatearPorcentaje(100)

    // Assert
    expect(resultado).toBe('100 %')
  })
})

describe('formatearDelta', () => {
  it('+12 → «+12»', () => {
    // Arrange y Act
    const resultado = formatearDelta(12)

    // Assert
    expect(resultado).toBe('+12')
  })
  it('-4 → «−4» (menos tipográfico)', () => {
    // Arrange y Act
    const resultado = formatearDelta(-4)

    // Assert
    expect(resultado).toMatch(/^[−-]4$/)
  })
  it('0 → «0»', () => {
    // Arrange y Act
    const resultado = formatearDelta(0)

    // Assert
    expect(resultado).toBe('0')
  })
})

describe('formatearDeltaPts', () => {
  it('+4 → «+4 pts»', () => {
    // Arrange y Act
    const resultado = formatearDeltaPts(4)

    // Assert
    expect(resultado).toBe('+4 pts')
  })
  it('+1 → «+1 pt»', () => {
    // Arrange y Act
    const resultado = formatearDeltaPts(1)

    // Assert
    expect(resultado).toBe('+1 pt')
  })
  it('-1 → «−1 pt»', () => {
    // Arrange y Act
    const resultado = formatearDeltaPts(-1)

    // Assert
    expect(resultado).toMatch(/^[−-]1 pt$/)
  })
})
