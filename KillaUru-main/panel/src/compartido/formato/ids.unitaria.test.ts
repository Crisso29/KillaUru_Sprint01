import { describe, it, expect } from 'vitest'
import { idCorto } from './ids'

// T014 — id corto de 8 caracteres

describe('idCorto', () => {
  it('UUID completo → primeros 8 chars', () => {
    // Arrange y Act
    const resultado = idCorto('a1b2c3d4-0000-0000-0000-000000000001')

    // Assert
    expect(resultado).toBe('a1b2c3d4')
  })

  it('cadena sin guiones', () => {
    // Arrange y Act
    const resultado = idCorto('abcdef1234567890')

    // Assert
    expect(resultado).toBe('abcdef12')
  })

  it('cadena corta → devuelve lo que hay', () => {
    // Arrange y Act
    const resultado = idCorto('abc')

    // Assert
    expect(resultado).toBe('abc')
  })
})
