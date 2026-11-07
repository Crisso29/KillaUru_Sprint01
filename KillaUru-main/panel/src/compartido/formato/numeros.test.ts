import { describe, it, expect } from 'vitest'
import { formatearNumero, formatearPorcentaje, formatearDelta, formatearDeltaPts } from './numeros'

// T014 — «287,4» · «8,1 %» · «+12» · «−4» · «+4 pts» · «+1 pt»

describe('formatearNumero', () => {
  it('287.4 → «287,4»', () => expect(formatearNumero(287.4)).toBe('287,4'))
  it('entero sin decimales', () => expect(formatearNumero(1000)).toBe('1000'))
  it('cero', () => expect(formatearNumero(0)).toBe('0'))
})

describe('formatearPorcentaje', () => {
  it('8.1 → «8,1 %»', () => expect(formatearPorcentaje(8.1)).toBe('8,1 %'))
  it('100 → «100 %»', () => expect(formatearPorcentaje(100)).toBe('100 %'))
})

describe('formatearDelta', () => {
  it('+12 → «+12»', () => expect(formatearDelta(12)).toBe('+12'))
  it('-4 → «−4» (menos tipográfico)', () => {
    const r = formatearDelta(-4)
    expect(r).toMatch(/^[−-]4$/)
  })
  it('0 → «0»', () => expect(formatearDelta(0)).toBe('0'))
})

describe('formatearDeltaPts', () => {
  it('+4 → «+4 pts»', () => expect(formatearDeltaPts(4)).toBe('+4 pts'))
  it('+1 → «+1 pt»', () => expect(formatearDeltaPts(1)).toBe('+1 pt'))
  it('-1 → «−1 pt»', () => {
    const r = formatearDeltaPts(-1)
    expect(r).toMatch(/^[−-]1 pt$/)
  })
})
