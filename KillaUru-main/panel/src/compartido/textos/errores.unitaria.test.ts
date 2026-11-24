import { describe, it, expect } from 'vitest'
import { mensajeDeError } from './errores'
import type { ErrorApi } from '@/api/problema'

// T013 — un mensaje por cada fila de data-model.md §2

describe('mensajeDeError', () => {
  const problema = (codigo: string, status = 400, extra?: Partial<ErrorApi>) =>
    mensajeDeError({ status, codigo, ...extra } as ErrorApi)

  it('401 credenciales_invalidas', () => {
    // Arrange y Act
    const resultado = problema('credenciales_invalidas', 401)

    // Assert
    expect(resultado).toBe('Correo o contraseña incorrectos.')
  })

  it('401 sesion_invalida', () => {
    // Arrange y Act
    const resultado = problema('sesion_invalida', 401)

    // Assert
    expect(resultado).toBe('Tu sesión terminó. Vuelve a entrar.')
  })

  it('401 cuenta_desactivada', () => {
    // Arrange y Act
    const resultado = problema('cuenta_desactivada', 401)

    // Assert
    expect(resultado).toBe('Tu sesión terminó. Vuelve a entrar.')
  })

  it('403 sin_permiso', () => {
    // Arrange y Act
    const resultado = problema('sin_permiso', 403)

    // Assert
    expect(resultado).toBe('No tienes permiso para esto.')
  })

  it('403 solo_administradores', () => {
    // Arrange y Act
    const resultado = problema('solo_administradores', 403)

    // Assert
    expect(resultado).toBe('Esta cuenta no tiene acceso al panel.')
  })

  it('403 antibot_fallido', () => {
    // Arrange y Act
    const resultado = problema('antibot_fallido', 403)

    // Assert
    expect(resultado)
      .toBe('No pudimos comprobar que eres una persona. Vuelve a intentarlo.')
  })

  it('404 no_encontrado (anomalía)', () => {
    // Arrange y Act
    const resultado = problema('no_encontrado', 404)

    // Assert
    expect(resultado).toContain('ya no existe')
  })

  it('409 transicion_no_permitida', () => {
    // Arrange y Act
    const resultado = problema('transicion_no_permitida', 409)

    // Assert
    expect(resultado)
      .toContain('anomalía cambió de estado')
  })

  it('409 correo_en_uso', () => {
    // Arrange y Act
    const resultado = problema('correo_en_uso', 409)

    // Assert
    expect(resultado).toBe('Ese correo ya tiene una cuenta.')
  })

  it('409 accion_sobre_si_mismo', () => {
    // Arrange y Act
    const resultado = problema('accion_sobre_si_mismo', 409)

    // Assert
    expect(resultado)
      .toContain('No puedes cambiar tu propio rol')
  })

  it('409 ultimo_administrador', () => {
    // Arrange y Act
    const resultado = problema('ultimo_administrador', 409)

    // Assert
    expect(resultado)
      .toBe('Debe quedar al menos un administrador activo.')
  })

  it('400 revision_incompleta', () => {
    // Arrange y Act
    const resultado = problema('revision_incompleta', 400)

    // Assert
    expect(resultado)
      .toContain('lista de administradores cambió')
  })

  it('400 enlace_invalido', () => {
    // Arrange y Act
    const resultado = problema('enlace_invalido', 400)

    // Assert
    expect(resultado).toContain('enlace no es válido')
  })

  it('400 datos_invalidos', () => {
    // Arrange y Act
    const resultado = problema('datos_invalidos', 400)

    // Assert
    expect(resultado).toBe('Revisa los datos marcados.')
  })

  it('429 limite_excedido', () => {
    // Arrange y Act
    const resultado = problema('limite_excedido', 429)

    // Assert
    expect(resultado).toContain('muchas consultas')
  })

  it('0 sin_conexion', () => {
    // Arrange y Act
    const resultado = problema('sin_conexion', 0)

    // Assert
    expect(resultado).toContain('No pudimos conectar')
  })

  it('5xx → incluye correlacionId', () => {
    // Arrange y Act
    const msg = mensajeDeError({ status: 500, codigo: 'error_interno', correlacionId: 'ABC-123' })

    // Assert
    expect(msg).toContain('ABC-123')
  })

  it('5xx sin correlacionId → mensaje genérico', () => {
    // Arrange y Act
    const msg = mensajeDeError({ status: 502, codigo: 'error_interno' })

    // Assert
    expect(msg).toContain('Algo falló')
  })

  it('un código desconocido que coincide con algo heredado («constructor») da el mensaje genérico', () => {
    // Arrange
    const error: ErrorApi = { status: 500, codigo: 'constructor' }

    // Act
    const mensaje = mensajeDeError(error)

    // Assert
    expect(mensaje).toBe('Algo falló de nuestro lado. Vuelve a intentarlo.')
  })
})
