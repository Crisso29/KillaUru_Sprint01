import { describe, it, expect } from 'vitest'
import { idCorto } from './ids'

// T014 — id corto de 8 caracteres

describe('idCorto', () => {
  it('UUID completo → primeros 8 chars', () =>
    expect(idCorto('a1b2c3d4-0000-0000-0000-000000000001')).toBe('a1b2c3d4'))

  it('cadena sin guiones', () =>
    expect(idCorto('abcdef1234567890')).toBe('abcdef12'))

  it('cadena corta → devuelve lo que hay', () =>
    expect(idCorto('abc')).toBe('abc'))
})
