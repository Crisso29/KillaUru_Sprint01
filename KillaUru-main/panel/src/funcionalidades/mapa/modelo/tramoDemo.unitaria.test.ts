import { describe, it, expect } from 'vitest'
import { dentroDelTramo, TRAMO_DEMO_KM } from './tramoDemo'

// T037 — tramoDemo: límites del corredor demo (R-012)

describe('TRAMO_DEMO_KM', () => {
  it('desde = 268, hasta = 335.073', () => {
    expect(TRAMO_DEMO_KM.desde).toBe(268)
    expect(TRAMO_DEMO_KM.hasta).toBe(335.073)
  })
})

describe('dentroDelTramo', () => {
  it('267,9 → fuera', () => {
    expect(dentroDelTramo(267.9)).toBe(false)
  })

  it('268 → dentro (límite inferior)', () => {
    expect(dentroDelTramo(268)).toBe(true)
  })

  it('300 → dentro', () => {
    expect(dentroDelTramo(300)).toBe(true)
  })

  it('335.073 → dentro (límite superior)', () => {
    expect(dentroDelTramo(335.073)).toBe(true)
  })

  it('335,1 → fuera', () => {
    expect(dentroDelTramo(335.1)).toBe(false)
  })
})
