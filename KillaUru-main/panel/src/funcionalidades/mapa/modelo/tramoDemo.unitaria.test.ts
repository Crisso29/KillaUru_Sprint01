// T037 — tramoDemo: límites del corredor demo (R-012)
import { describe, it, expect } from 'vitest'
import { dentroDelTramo, TRAMO_DEMO_KM } from './tramoDemo'

describe('TRAMO_DEMO_KM', () => {
  it('va de Vinchos (km 268) al fin del corredor (km 335,073)', () => {
    // Arrange y Act
    const { desde, hasta } = TRAMO_DEMO_KM

    // Assert
    expect({ desde, hasta }).toEqual({ desde: 268, hasta: 335.073 })
  })
})

describe('dentroDelTramo', () => {
  it.each([
    [267.9, false],
    [268, true], // límite inferior incluido
    [300, true],
    [335.073, true], // límite superior incluido
    [335.1, false],
  ])('km %s → dentro: %s', (km, esperado) => {
    // Arrange y Act
    const dentro = dentroDelTramo(km)

    // Assert
    expect(dentro).toBe(esperado)
  })
})
