import { describe, it, expect } from 'vitest'
import { filtrosAConsulta } from './peticion'
import { filtrosDefecto } from './filtros'

// T036 — filtrosAConsulta: estado siempre, tipo solo con uno, severidad (lista) omitida con 5, desde fijo

const AHORA = new Date('2026-11-05T12:00:00.000Z')

describe('filtrosAConsulta', () => {
  it('estado siempre presente (dos estados)', () => {
    // Arrange y Act
    const consulta = filtrosAConsulta(filtrosDefecto(), AHORA)

    // Assert
    expect(consulta.has('estado')).toBe(true)
    expect(consulta.get('estado')).toBe('Activa,PendienteConfirmacion')
  })

  it('tipo ausente con ambos tipos', () => {
    // Arrange y Act
    const consulta = filtrosAConsulta(filtrosDefecto(), AHORA)

    // Assert
    expect(consulta.has('tipo')).toBe(false)
  })

  it('tipo presente con un solo tipo', () => {
    // Arrange
    const filtros = { ...filtrosDefecto(), tipos: ['Bache'] as const }

    // Act
    const consulta = filtrosAConsulta(filtros, AHORA)

    // Assert
    expect(consulta.get('tipo')).toBe('Bache')
  })

  it('con las 5 severidades no se filtra por severidad', () => {
    // Arrange y Act
    const consulta = filtrosAConsulta(filtrosDefecto(), AHORA)

    // Assert
    expect(consulta.has('severidad')).toBe(false)
  })

  // Contrato 006 (mapa.openapi.yaml): `severidad` es una lista separada por comas. Hasta el 2026-11-23 el panel enviaba
  // `severidad_min`, que el backend no conoce: el filtro de severidad no tenía efecto contra la API real.
  it.each([
    [[3, 4, 5], '3,4,5'],
    [[2, 5], '2,5'], // no contiguas: un «mínimo 2» traería también 3 y 4
  ])('severidades %j → severidad=%s, como pide el contrato', (severidades, esperado) => {
    // Arrange
    const filtros = { ...filtrosDefecto(), severidades }

    // Act
    const consulta = filtrosAConsulta(filtros, AHORA)

    // Assert
    expect({ severidad: consulta.get('severidad'), severidad_min: consulta.get('severidad_min') }).toEqual({ severidad: esperado, severidad_min: null })
  })

  it('desde = ahora − 24h', () => {
    // Arrange
    const filtros = { ...filtrosDefecto(), periodo: '24h' as const }

    // Act
    const consulta = filtrosAConsulta(filtros, AHORA)

    // Assert
    expect(consulta.get('desde')).toBe('2026-11-04T12:00:00.000Z')
  })

  it('desde = ahora − 7d', () => {
    // Arrange y Act
    const consulta = filtrosAConsulta(filtrosDefecto(), AHORA)

    // Assert
    expect(consulta.get('desde')).toBe('2026-10-29T12:00:00.000Z')
  })

  it('desde = ahora − 30d', () => {
    // Arrange
    const filtros = { ...filtrosDefecto(), periodo: '30d' as const }

    // Act
    const consulta = filtrosAConsulta(filtros, AHORA)

    // Assert
    expect(consulta.get('desde')).toBe('2026-10-06T12:00:00.000Z')
  })

  it('nunca bbox ni updatedSince', () => {
    // Arrange y Act
    const consulta = filtrosAConsulta(filtrosDefecto(), AHORA)

    // Assert
    expect(consulta.has('bbox')).toBe(false)
    expect(consulta.has('updatedSince')).toBe(false)
  })
})
