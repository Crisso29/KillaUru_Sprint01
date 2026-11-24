import { describe, it, expect } from 'vitest'
import { ERROR_INESPERADO, mensajeDeErrorAccion } from './erroresAccion'
import { TEXTOS_USUARIOS as textos } from '../textos'

// T057 — un solo lugar decide qué ve el administrador ante cada 409 (antes, una cadena de if en cada diálogo)

describe('mensajeDeErrorAccion', () => {
  it.each([
    ['accion_sobre_si_mismo', textos.errorAccionSobreSiMismo],
    ['ultimo_administrador', textos.errorUltimoAdministrador],
    ['correo_en_uso', textos.errorCorreoEnUso],
    ['revision_incompleta', textos.errorRevisionIncompleta],
  ])('%s tiene su mensaje', (codigo, esperado) => {
    // Arrange y Act
    const mensaje = mensajeDeErrorAccion(codigo)

    // Assert
    expect(mensaje).toBe(esperado)
  })

  it('un código desconocido da el mensaje genérico, nunca el código crudo', () => {
    // Arrange
    const codigo = 'codigo_que_no_existe'

    // Act
    const mensaje = mensajeDeErrorAccion(codigo)

    // Assert
    expect(mensaje).toBe(ERROR_INESPERADO)
  })
})
