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
  it('km 0 → tramo 0', () => {
    expect(calcularTramo(0)).toBe(0)
  })

  it('km 9,99 → tramo 0', () => {
    expect(calcularTramo(9.99)).toBe(0)
  })

  it('km 10 → tramo 1', () => {
    expect(calcularTramo(10)).toBe(1)
  })

  it('km 332,9 → tramo 33', () => {
    expect(calcularTramo(332.9)).toBe(33)
  })

  it('km 335,073 se recorta a 332,999 → tramo 33', () => {
    expect(calcularTramo(335.073)).toBe(33)
  })
})

describe('agruparEnFranja', () => {
  it('agrupa por tramo con severidad máxima', () => {
    const anomalias = [anomalia(10, 2), anomalia(15, 4), anomalia(25, 3)]
    const franjas = agruparEnFranja(anomalias)
    const tramo1 = franjas.find((f) => f.tramo === 1)
    expect(tramo1?.cantidad).toBe(2)
    expect(tramo1?.severidadMax).toBe(4)
  })

  it('tramo sin anomalías → no aparece en el array', () => {
    const franjas = agruparEnFranja([anomalia(5, 1)])
    expect(franjas.some((f) => f.tramo === 1)).toBe(false)
  })

  it('anomalía única → severidadMax igual a su severidad', () => {
    const franjas = agruparEnFranja([anomalia(287.4, 3)])
    const tramo = franjas.find((f) => f.tramo === calcularTramo(287.4))
    expect(tramo?.severidadMax).toBe(3)
  })

  it('lista vacía → array vacío', () => {
    expect(agruparEnFranja([])).toHaveLength(0)
  })
})
