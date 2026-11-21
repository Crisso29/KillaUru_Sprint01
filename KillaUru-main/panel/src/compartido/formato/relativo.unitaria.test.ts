import { describe, it, expect } from 'vitest'
import { formatearRelativo } from './relativo'

// T014 — «hace 2 h», «ayer a las 15:30»

describe('formatearRelativo', () => {
  const ahora = new Date('2026-11-05T17:00:00Z')

  it('hace 2 h (dentro del mismo día)', () => {
    const ts = new Date('2026-11-05T15:00:00Z').toISOString()
    expect(formatearRelativo(ts, ahora)).toBe('hace 2 h')
  })

  it('hace menos de 1 min → «ahora mismo»', () => {
    const ts = new Date(ahora.getTime() - 30_000).toISOString()
    expect(formatearRelativo(ts, ahora)).toBe('ahora mismo')
  })

  it('ayer a las 15:30 (Lima)', () => {
    // ayer en Lima: 2026-11-04; 15:30 Lima = 20:30 UTC
    const ts = '2026-11-04T20:30:00Z'
    // ahora Lima: 2026-11-05T12:00 (17:00 UTC - 5h)
    expect(formatearRelativo(ts, ahora)).toBe('ayer a las 15:30')
  })

  it('hace 3 días', () => {
    const ts = new Date(ahora.getTime() - 3 * 24 * 3600_000).toISOString()
    expect(formatearRelativo(ts, ahora)).toMatch(/hace 3 d/)
  })
})
