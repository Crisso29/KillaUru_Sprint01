// T059 — validarContrasena: 9/10/128/129 chars; validarConfirmacion: iguales/distintas
import { describe, it, expect } from 'vitest'
import { validarContrasena, validarConfirmacion } from './contrasena'

describe('validarContrasena', () => {
  it.each([
    [9, false],
    [10, true], // mínimo permitido
    [128, true], // máximo permitido
    [129, false],
  ])('%i caracteres → válida: %s', (largo, valida) => {
    // Arrange
    const contrasena = 'a'.repeat(largo)

    // Act
    const error = validarContrasena(contrasena)

    // Assert
    expect(error === null).toBe(valida)
  })
})

describe('validarConfirmacion', () => {
  it.each([
    ['iguales', 'MiClave1234', true],
    ['distintas', 'OtraClave1234', false],
  ])('%s → válida: %s', (_caso, confirmacion, valida) => {
    // Arrange y Act
    const error = validarConfirmacion('MiClave1234', confirmacion)

    // Assert
    expect(error === null).toBe(valida)
  })
})
