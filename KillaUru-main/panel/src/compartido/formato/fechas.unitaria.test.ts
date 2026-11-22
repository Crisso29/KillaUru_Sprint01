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

// Prototipo, subtítulo del Resumen: «Lunes 20 de octubre · hora de Lima». Patrón AAA.
describe('formatearDiaLargoLima', () => {
  it('escribe el día de la semana con mayúscula, el día y el mes', async () => {
    // Arrange
    const { formatearDiaLargoLima } = await import('./fechas')
    // Act + Assert
    expect(formatearDiaLargoLima('2026-10-20T15:00:00Z')).toBe('Martes 20 de octubre')
  })

  it('usa el día de Lima: las 02:00 UTC todavía son el día anterior allá', async () => {
    // Arrange
    const { formatearDiaLargoLima } = await import('./fechas')
    // Act + Assert
    expect(formatearDiaLargoLima('2026-11-23T02:00:00Z')).toBe('Domingo 22 de noviembre')
  })
})

describe('formatearUtcCorto', () => {
  it('escribe día, mes abreviado y hora en UTC', async () => {
    // Arrange
    const { formatearUtcCorto } = await import('./fechas')
    // Act + Assert
    expect(formatearUtcCorto('2026-10-20T10:12:00Z')).toBe('20 oct 10:12 UTC')
  })
})
