import { describe, it, expect } from 'vitest'
import { armarError } from './problema'

// T013 — Problema / ProblemaBloqueo → ErrorApi; sin cuerpo → sin_conexion

describe('armarError', () => {
  it('body con status y codigo', async () => {
    const r = new Response(
      JSON.stringify({ codigo: 'sin_permiso', correlacion_id: 'corr-123' }),
      { status: 403 },
    )
    const e = await armarError(r)
    expect(e.status).toBe(403)
    expect(e.codigo).toBe('sin_permiso')
    expect(e.correlacionId).toBe('corr-123')
  })

  it('ProblemaBloqueo → bloqueadoHastaUtc', async () => {
    const r = new Response(
      JSON.stringify({ codigo: 'cuenta_bloqueada', bloqueado_hasta_utc: '2026-11-05T10:00:00Z' }),
      { status: 423 },
    )
    const e = await armarError(r)
    expect(e.bloqueadoHastaUtc).toEqual(new Date('2026-11-05T10:00:00Z'))
  })

  it('sin cuerpo legible → sin_conexion', async () => {
    // status 0 no es constructible en JSDOM; usamos 500 sin body
    const r = new Response('', { status: 500 })
    const e = await armarError(r)
    expect(e.status).toBe(500)
    expect(e.codigo).toBe('sin_conexion')
  })

  it('JSON inválido → sin_conexion', async () => {
    const r = new Response('no-json', { status: 500 })
    const e = await armarError(r)
    expect(e.codigo).toBe('sin_conexion')
  })

  it('5xx con correlacionId en el mensaje', async () => {
    const r = new Response(
      JSON.stringify({ codigo: 'error_interno', correlacion_id: 'XYZ-999' }),
      { status: 503 },
    )
    const e = await armarError(r)
    expect(e.correlacionId).toBe('XYZ-999')
    expect(e.status).toBe(503)
  })
})
