import { describe, it, expect } from 'vitest'
import { marcaParaEstado, tieneAnillo, claveIcono, ESTADOS_MAPA, SEVERIDADES } from './simbolos'

// T038 — simbolos: formas únicas por estado, anillo en Activa 4-5, 25 combinaciones

const ESTADOS = ['Activa', 'PendienteConfirmacion', 'Expirada', 'Reparada', 'Descartada']

describe('marcaParaEstado', () => {
  // FR-023: «Activa: círculo lleno, con anillo en 4 y 5; Pendiente: borde punteado; Expirada: anillo gris;
  // Reparada: círculo con ✓; Descartada: círculo con ✕», siempre con su número de severidad.
  it('cada estado tiene la marca que pide FR-023', () => {
    // Arrange + Act
    const marcas = ESTADOS_MAPA.map(marcaParaEstado)
    // Assert
    expect(marcas).toEqual(['lleno', 'punteado', 'anillo', 'check', 'cruz'])
  })

  it('cada estado tiene una marca distinta', () => {
    const formas = ESTADOS.map(marcaParaEstado)
    expect(new Set(formas).size).toBe(5)
  })

  it('las constantes cubren los 5 estados', () => {
    expect(ESTADOS_MAPA).toHaveLength(5)
    expect(SEVERIDADES).toEqual([1, 2, 3, 4, 5])
  })
})

describe('tieneAnillo', () => {
  it('Activa 4 → con anillo', () => {
    expect(tieneAnillo('Activa', 4)).toBe(true)
  })

  it('Activa 5 → con anillo', () => {
    expect(tieneAnillo('Activa', 5)).toBe(true)
  })

  it('Activa 3 → sin anillo', () => {
    expect(tieneAnillo('Activa', 3)).toBe(false)
  })

  it('PendienteConfirmacion 5 → sin anillo', () => {
    expect(tieneAnillo('PendienteConfirmacion', 5)).toBe(false)
  })

  it('Expirada 4 → sin anillo', () => {
    expect(tieneAnillo('Expirada', 4)).toBe(false)
  })
})

describe('claveIcono — 25 combinaciones', () => {
  it('todas llevan el número de severidad', () => {
    for (const estado of ESTADOS) {
      for (const sev of [1, 2, 3, 4, 5]) {
        const clave = claveIcono(estado, sev)
        expect(clave).toContain(String(sev))
      }
    }
  })

  it('las 25 claves son únicas', () => {
    const claves = ESTADOS.flatMap(e => [1, 2, 3, 4, 5].map(s => claveIcono(e, s)))
    expect(new Set(claves).size).toBe(25)
  })

  it('Activa 4 y 5 incluyen «anillo» en la clave', () => {
    expect(claveIcono('Activa', 4)).toContain('anillo')
    expect(claveIcono('Activa', 5)).toContain('anillo')
  })

  it('Activa 3 no incluye «anillo»', () => {
    expect(claveIcono('Activa', 3)).not.toContain('anillo')
  })
})
