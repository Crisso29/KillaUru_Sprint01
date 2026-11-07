import { describe, it, expect } from 'vitest'
import { mensajeDeError } from './errores'
import type { ErrorApi } from '@/api/problema'

// T013 — un mensaje por cada fila de data-model.md §2

describe('mensajeDeError', () => {
  const c = (codigo: string, status = 400, extra?: Partial<ErrorApi>) =>
    mensajeDeError({ status, codigo, ...extra } as ErrorApi)

  it('401 credenciales_invalidas', () =>
    expect(c('credenciales_invalidas', 401)).toBe('Correo o contraseña incorrectos.'))

  it('401 sesion_invalida', () =>
    expect(c('sesion_invalida', 401)).toBe('Tu sesión terminó. Vuelve a entrar.'))

  it('401 cuenta_desactivada', () =>
    expect(c('cuenta_desactivada', 401)).toBe('Tu sesión terminó. Vuelve a entrar.'))

  it('403 sin_permiso', () =>
    expect(c('sin_permiso', 403)).toBe('No tienes permiso para esto.'))

  it('403 solo_administradores', () =>
    expect(c('solo_administradores', 403)).toBe('Esta cuenta no tiene acceso al panel.'))

  it('403 antibot_fallido', () =>
    expect(c('antibot_fallido', 403))
      .toBe('No pudimos comprobar que eres una persona. Vuelve a intentarlo.'))

  it('404 no_encontrado (anomalía)', () =>
    expect(c('no_encontrado', 404)).toContain('ya no existe'))

  it('409 transicion_no_permitida', () =>
    expect(c('transicion_no_permitida', 409))
      .toContain('anomalía cambió de estado'))

  it('409 correo_en_uso', () =>
    expect(c('correo_en_uso', 409)).toBe('Ese correo ya tiene una cuenta.'))

  it('409 accion_sobre_si_mismo', () =>
    expect(c('accion_sobre_si_mismo', 409))
      .toContain('No puedes cambiar tu propio rol'))

  it('409 ultimo_administrador', () =>
    expect(c('ultimo_administrador', 409))
      .toBe('Debe quedar al menos un administrador activo.'))

  it('400 revision_incompleta', () =>
    expect(c('revision_incompleta', 400))
      .toContain('lista de administradores cambió'))

  it('400 enlace_invalido', () =>
    expect(c('enlace_invalido', 400)).toContain('enlace no es válido'))

  it('400 datos_invalidos', () =>
    expect(c('datos_invalidos', 400)).toBe('Revisa los datos marcados.'))

  it('429 limite_excedido', () =>
    expect(c('limite_excedido', 429)).toContain('muchas consultas'))

  it('0 sin_conexion', () =>
    expect(c('sin_conexion', 0)).toContain('No pudimos conectar'))

  it('5xx → incluye correlacionId', () => {
    const msg = mensajeDeError({ status: 500, codigo: 'error_interno', correlacionId: 'ABC-123' })
    expect(msg).toContain('ABC-123')
  })

  it('5xx sin correlacionId → mensaje genérico', () => {
    const msg = mensajeDeError({ status: 502, codigo: 'error_interno' })
    expect(msg).toContain('Algo falló')
  })
})
