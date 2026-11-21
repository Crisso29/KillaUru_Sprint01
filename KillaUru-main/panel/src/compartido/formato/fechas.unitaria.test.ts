import { describe, it, expect } from 'vitest'
import { formatearFechaLima, formatearHoraLima } from './fechas'

// T014 — Lima = UTC-5 (sin horario de verano)
// 2026-10-21T04:59Z → 20 oct en Lima
// 2026-10-21T05:00Z → 21 oct en Lima

describe('formatearFechaLima', () => {
  it('04:59Z → 20 oct (Lima = UTC-5)', () =>
    expect(formatearFechaLima('2026-10-21T04:59:00Z')).toBe('20 oct'))

  it('05:00Z → 21 oct (Lima = UTC-5)', () =>
    expect(formatearFechaLima('2026-10-21T05:00:00Z')).toBe('21 oct'))

  it('mismo día en Lima', () =>
    expect(formatearFechaLima('2026-11-05T12:00:00Z')).toBe('5 nov'))
})

describe('formatearHoraLima', () => {
  it('convierte UTC a hora Lima', () => {
    // 20:00 UTC → 15:00 Lima
    expect(formatearHoraLima('2026-11-05T20:00:00Z')).toBe('15:00')
  })
})
