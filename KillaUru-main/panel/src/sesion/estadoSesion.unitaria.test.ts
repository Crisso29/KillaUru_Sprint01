// T015 — transiciones de la sesión del panel (data-model.md §1). Se compara el estado COMPLETO resultante, no solo su
// tipo: así una transición que pierde el correo o el motivo también falla.
import { describe, it, expect } from 'vitest'
import { reducirSesion, type AccionSesion, type EstadoSesion } from './estadoSesion'

const EXPIRA = new Date('2026-11-05T10:05:00Z')
const VENCE = new Date('2026-11-05T18:00:00Z')
const TIEMPOS = { expiraAccesoUtc: EXPIRA, sesionVenceUtc: VENCE, desfaseMs: 240_000 }

const DESCONOCIDA: EstadoSesion = { tipo: 'desconocida' }
const ANONIMA: EstadoSesion = { tipo: 'anonima' }
const ACTIVA: EstadoSesion = { tipo: 'activa', ...TIEMPOS, correo: 'ops@killauru.pe' }

const NUEVOS = { expiraAccesoUtc: new Date('2026-11-05T10:10:00Z'), sesionVenceUtc: VENCE, desfaseMs: 300_000 }

describe('reducirSesion', () => {
  it.each<[string, EstadoSesion, AccionSesion, EstadoSesion]>([
    ['al abrir con cookie viva → activa (el correo llega después)', DESCONOCIDA, { tipo: 'renovar_ok', ...TIEMPOS },
      { tipo: 'activa', ...TIEMPOS, correo: undefined }],
    ['al abrir sin cookie → anónima sin motivo (no hubo sesión que perder)', DESCONOCIDA, { tipo: 'renovar_401' },
      { tipo: 'anonima', motivo: undefined }],
    ['login correcto → activa', ANONIMA, { tipo: 'login_ok', ...TIEMPOS }, { tipo: 'activa', ...TIEMPOS, correo: undefined }],
    ['renovar → nuevos tiempos y conserva el correo ya conocido', ACTIVA, { tipo: 'renovar_ok', ...NUEVOS },
      { tipo: 'activa', ...NUEVOS, correo: 'ops@killauru.pe' }],
    ['renovación rechazada → anónima por sesión perdida', ACTIVA, { tipo: 'renovar_401' }, { tipo: 'anonima', motivo: 'sesion' }],
    // La renovación rechazada y el reintento del pedido avisan los dos (E2E sesion.spec): el segundo aviso no debe
    // borrar el motivo, o el login no explica por qué hay que volver a entrar
    ['un segundo 401 con la sesión ya perdida conserva el motivo', { tipo: 'anonima', motivo: 'sesion' }, { tipo: 'renovar_401' },
      { tipo: 'anonima', motivo: 'sesion' }],
    ['salir → anónima por salida', ACTIVA, { tipo: 'salir' }, { tipo: 'anonima', motivo: 'salida' }],
    ['llega el correo → activa con correo', { ...ACTIVA, correo: undefined }, { tipo: 'correo_ok', correo: 'ops@killauru.pe' }, ACTIVA],
    ['correo sin sesión → no cambia nada', ANONIMA, { tipo: 'correo_ok', correo: 'ops@killauru.pe' }, ANONIMA],
  ])('%s', (_caso, estado, accion, esperado) => {
    // Arrange y Act
    const siguiente = reducirSesion(estado, accion)

    // Assert
    expect(siguiente).toEqual(esperado)
  })
})
