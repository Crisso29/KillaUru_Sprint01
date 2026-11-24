// T038 — simbolos: formas únicas por estado, anillo en Activa 4-5, 25 combinaciones
import { describe, it, expect } from 'vitest'
import { marcaParaEstado, tieneAnillo, claveIcono, ESTADOS_MAPA, SEVERIDADES } from './simbolos'

describe('marcaParaEstado', () => {
  // FR-023: «Activa: círculo lleno, con anillo en 4 y 5; Pendiente: borde punteado; Expirada: anillo gris;
  // Reparada: círculo con ✓; Descartada: círculo con ✕», siempre con su número de severidad.
  it('cada estado tiene la marca que pide FR-023, y ninguna se repite', () => {
    // Arrange
    const estados = ESTADOS_MAPA

    // Act
    const marcas = estados.map(marcaParaEstado)

    // Assert
    expect(marcas).toEqual(['lleno', 'punteado', 'anillo', 'check', 'cruz'])
    expect(new Set(marcas).size).toBe(estados.length)
  })

  it('las constantes cubren los 5 estados y las 5 severidades', () => {
    // Arrange y Act
    const cantidades = { estados: ESTADOS_MAPA.length, severidades: [...SEVERIDADES] }

    // Assert
    expect(cantidades).toEqual({ estados: 5, severidades: [1, 2, 3, 4, 5] })
  })
})

describe('tieneAnillo', () => {
  it.each([
    ['Activa', 4, true],
    ['Activa', 5, true],
    ['Activa', 3, false],
    ['PendienteConfirmacion', 5, false],
    ['Expirada', 4, false],
  ])('%s %i → anillo: %s', (estado, severidad, esperado) => {
    // Arrange y Act
    const conAnillo = tieneAnillo(estado, severidad)

    // Assert
    expect(conAnillo).toBe(esperado)
  })
})

describe('claveIcono — 25 combinaciones', () => {
  it('las 25 claves son únicas y cada una lleva su número de severidad', () => {
    // Arrange
    const combinaciones = ESTADOS_MAPA.flatMap((estado) => SEVERIDADES.map((severidad) => ({ estado, severidad })))

    // Act
    const claves = combinaciones.map(({ estado, severidad }) => ({ clave: claveIcono(estado, severidad), severidad }))

    // Assert
    expect(new Set(claves.map(({ clave }) => clave)).size).toBe(25)
    claves.forEach(({ clave, severidad }) => expect(clave).toContain(String(severidad)))
  })

  it.each([
    [4, true],
    [5, true],
    [3, false],
  ])('Activa %i: la clave nombra el anillo → %s', (severidad, esperado) => {
    // Arrange y Act
    const clave = claveIcono('Activa', severidad)

    // Assert
    expect(clave.includes('anillo')).toBe(esperado)
  })
})
