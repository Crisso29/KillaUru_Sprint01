// T070 — franja: tramo correcto y severidad máxima (data-model §5, FR-019, R-013)
import { describe, it, expect } from 'vitest'
import { calcularTramo, agruparEnFranja } from './franja'
import type { AnomaliaFeature } from '@/api/dtos/002-mapa'

function anomalia(kmVia: number, severidad: number): AnomaliaFeature {
  return {
    type: 'Feature',
    id: 'x',
    geometry: { type: 'Point', coordinates: [-75, -14] },
    properties: {
      id: 'x',
      kmVia,
      tipo: 'Bache',
      severidad: severidad as 1 | 2 | 3 | 4 | 5,
      persistencia: 'media',
      ultimaDeteccionUtc: '2026-11-01T00:00:00Z',
      venceUtc: null,
      estado: 'Activa',
    },
  }
}

describe('calcularTramo', () => {
  it.each([
    [0, 0],
    [9.99, 0],
    [10, 1],
    [332.9, 33],
    [335.073, 33], // el fin del corredor se recorta a 332,999: no existe un tramo 34 de 2 km
  ])('km %s → tramo %i', (km, esperado) => {
    // Arrange y Act
    const tramo = calcularTramo(km)

    // Assert
    expect(tramo).toBe(esperado)
  })
})

describe('agruparEnFranja', () => {
  it('agrupa por tramo con severidad máxima', () => {
    // Arrange
    const anomalias = [anomalia(10, 2), anomalia(15, 4), anomalia(25, 3)]
    // Act
    const franjas = agruparEnFranja(anomalias)

    // Assert
    const tramo1 = franjas.find((franja) => franja.tramo === 1)
    expect(tramo1?.cantidad).toBe(2)
    expect(tramo1?.severidadMax).toBe(4)
  })

  it('tramo sin anomalías → no aparece en el array', () => {
    // Arrange y Act
    const franjas = agruparEnFranja([anomalia(5, 1)])

    // Assert
    expect(franjas.some((franja) => franja.tramo === 1)).toBe(false)
  })

  it('anomalía única → severidadMax igual a su severidad', () => {
    // Arrange y Act
    const franjas = agruparEnFranja([anomalia(287.4, 3)])

    // Assert
    const tramo = franjas.find((franja) => franja.tramo === calcularTramo(287.4))
    expect(tramo?.severidadMax).toBe(3)
  })

  it('lista vacía → array vacío', () => {
    // Arrange y Act
    const franjas = agruparEnFranja([])

    // Assert
    expect(franjas).toHaveLength(0)
  })
})
