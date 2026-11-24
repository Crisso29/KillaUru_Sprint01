// T082 — aDesdeUtc / aHastaUtc: conversión fecha Lima → UTC
import { describe, it, expect } from 'vitest'
import { aDesdeUtc, aHastaUtc, construirSolicitud } from './solicitud'

describe('aDesdeUtc y aHastaUtc (Lima = UTC−5, sin horario de verano)', () => {
  it.each([
    ['desde: medianoche de Lima', aDesdeUtc, '2026-10-13', '2026-10-13T05:00:00Z'],
    ['hasta: fin del día en Lima', aHastaUtc, '2026-10-20', '2026-10-21T04:59:59Z'],
    ['desde vacío → sin límite', aDesdeUtc, '', null],
    ['hasta vacío → sin límite', aHastaUtc, '', null],
  ])('%s', (_caso, convertir, fecha, esperado) => {
    // Arrange y Act
    const utc = convertir(fecha)

    // Assert
    expect(utc).toBe(esperado)
  })
})

describe('construirSolicitud', () => {
  it('«Todos» los tipos y ningún estado → sin filtros de tipo ni de estado', () => {
    // Arrange y Act
    const solicitud = construirSolicitud('CSV', '', '', 'Todos', [])

    // Assert
    expect({ tipo: solicitud.tipo, estados: solicitud.estados }).toEqual({ tipo: null, estados: [] })
  })

  it('tipo, formato y rango elegidos pasan a la solicitud en UTC', () => {
    // Arrange y Act
    const solicitud = construirSolicitud('GeoJSON', '2026-10-13', '2026-10-20', 'Bache', [])

    // Assert
    expect(solicitud.tipo).toBe('Bache')
    expect(solicitud.formato).toBe('GeoJSON')
    expect(solicitud.desdeUtc).toBe('2026-10-13T05:00:00Z')
    expect(solicitud.hastaUtc).toBe('2026-10-21T04:59:59Z')
  })
})
