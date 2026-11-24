import { describe, it, expect } from 'vitest'
import { armarError } from './problema'

// T013 — Problema / ProblemaBloqueo → ErrorApi; sin cuerpo → sin_conexion.
// Los cuerpos son los del contrato (001 `Problema`, 003 `ProblemaBloqueo`): camelCase, como los manda la API.
// Hasta el 2026-11-23 esta prueba usaba `correlacion_id` y `bloqueado_hasta_utc`, que la API nunca envía: el aviso de
// bloqueo no mostraba la hora y los errores perdían su correlacionId.

function problema(status: number, extra: Record<string, unknown>): Response {
  const cuerpo = { type: 'https://api.killauru.pe/errores/x', title: 'Error', status, ...extra }
  return new Response(JSON.stringify(cuerpo), { status, headers: { 'Content-Type': 'application/problem+json' } })
}

describe('armarError', () => {
  it('lee status, codigo y correlacionId de un Problema', async () => {
    // Arrange
    const respuesta = problema(403, { codigo: 'sin_permiso', correlacionId: 'corr-123' })

    // Act
    const error = await armarError(respuesta)

    // Assert
    expect(error).toEqual({ status: 403, codigo: 'sin_permiso', correlacionId: 'corr-123', bloqueadoHastaUtc: undefined })
  })

  it('ProblemaBloqueo trae la hora de desbloqueo', async () => {
    // Arrange
    const respuesta = problema(423, { codigo: 'cuenta_bloqueada', correlacionId: 'c', bloqueadoHastaUtc: '2026-11-05T10:00:00Z' })

    // Act
    const error = await armarError(respuesta)

    // Assert
    expect(error.bloqueadoHastaUtc).toEqual(new Date('2026-11-05T10:00:00Z'))
  })

  it('sin cuerpo legible → sin_conexion', async () => {
    // Arrange — status 0 no es construible en JSDOM; se usa 500 sin cuerpo
    const respuesta = new Response('', { status: 500 })

    // Act
    const error = await armarError(respuesta)

    // Assert
    expect(error).toEqual({ status: 500, codigo: 'sin_conexion' })
  })

  it('JSON inválido → sin_conexion', async () => {
    // Arrange
    const respuesta = new Response('no-json', { status: 500 })

    // Act
    const error = await armarError(respuesta)

    // Assert
    expect(error.codigo).toBe('sin_conexion')
  })

  it('un 5xx conserva su correlacionId para que soporte lo rastree', async () => {
    // Arrange
    const respuesta = problema(503, { codigo: 'error_interno', correlacionId: 'XYZ-999' })

    // Act
    const error = await armarError(respuesta)

    // Assert
    expect([error.status, error.correlacionId]).toEqual([503, 'XYZ-999'])
  })
})
