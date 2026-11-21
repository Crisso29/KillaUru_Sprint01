// T059 — validarContrasena: 9/10/128/129 chars; validarConfirmacion: iguales/distintas
import { describe, it, expect } from 'vitest'
import { validarContrasena, validarConfirmacion } from './contrasena'

describe('validarContrasena', () => {
  it('9 caracteres → error', () => {
    expect(validarContrasena('a'.repeat(9))).not.toBeNull()
  })

  it('10 caracteres → válida', () => {
    expect(validarContrasena('a'.repeat(10))).toBeNull()
  })

  it('128 caracteres → válida', () => {
    expect(validarContrasena('a'.repeat(128))).toBeNull()
  })

  it('129 caracteres → error', () => {
    expect(validarContrasena('a'.repeat(129))).not.toBeNull()
  })
})

describe('validarConfirmacion', () => {
  it('iguales → null', () => {
    expect(validarConfirmacion('MiClave1234', 'MiClave1234')).toBeNull()
  })

  it('distintas → error', () => {
    expect(validarConfirmacion('MiClave1234', 'OtraClave1234')).not.toBeNull()
  })
})
