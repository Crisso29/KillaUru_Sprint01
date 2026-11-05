import { describe, it, expect } from 'vitest'
import { formatearKm } from './km'

// T014 — formato de km en el corredor

describe('formatearKm', () => {
  it('entero → sin decimales', () => expect(formatearKm(268)).toBe('268,0'))
  it('con decimal', () => expect(formatearKm(287.4)).toBe('287,4'))
  it('km 0', () => expect(formatearKm(0)).toBe('0,0'))
  it('fin corredor', () => expect(formatearKm(335.073)).toBe('335,1'))
})
