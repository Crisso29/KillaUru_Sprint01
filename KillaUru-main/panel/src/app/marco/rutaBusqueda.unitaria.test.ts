// T049 — a qué pantalla lleva cada búsqueda rápida (FR-040)
import { describe, it, expect } from 'vitest'
import { rutaDeBusqueda } from './rutaBusqueda'

describe('rutaDeBusqueda', () => {
  it.each([
    ['id completo', { tipo: 'id', id: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890' } as const, '/mapa?anomalia=a1b2c3d4-e5f6-7890-abcd-ef1234567890'],
    ['id corto de 8 (como se ve en tablas)', { tipo: 'id-prefijo', prefijo: 'a1b2c3d4' } as const, '/mapa?anomalia=a1b2c3d4'],
    ['km', { tipo: 'km', km: 287.4 } as const, '/mapa?kmVia=287.4'],
    ['correo (se codifica el «+»)', { tipo: 'correo', correo: 'ops+1@killauru.pe' } as const, '/usuarios?busqueda=ops%2B1%40killauru.pe'],
  ])('%s', (_caso, intento, esperada) => {
    // Arrange y Act
    const ruta = rutaDeBusqueda(intento)

    // Assert
    expect(ruta).toBe(esperada)
  })

  it('un texto que no se reconoce no lleva a ninguna parte', () => {
    // Arrange
    const intento = { tipo: 'sin_resultados' } as const

    // Act
    const ruta = rutaDeBusqueda(intento)

    // Assert
    expect(ruta).toBeNull()
  })
})
