// T082 — aDesdeUtc / aHastaUtc: conversión fecha Lima → UTC
import { describe, it, expect } from 'vitest'
import { aDesdeUtc, aHastaUtc, construirSolicitud } from './solicitud'

describe('aDesdeUtc', () => {
  it('fecha "2026-10-13" → "2026-10-13T05:00:00Z" (medianoche Lima = UTC-5)', () => {
    expect(aDesdeUtc('2026-10-13')).toBe('2026-10-13T05:00:00Z')
  })

  it('fecha vacía "" → null', () => {
    expect(aDesdeUtc('')).toBeNull()
  })
})

describe('aHastaUtc', () => {
  it('fecha "2026-10-20" → "2026-10-21T04:59:59Z" (fin del día Lima = UTC-5)', () => {
    expect(aHastaUtc('2026-10-20')).toBe('2026-10-21T04:59:59Z')
  })

  it('fecha vacía "" → null', () => {
    expect(aHastaUtc('')).toBeNull()
  })
})

describe('construirSolicitud', () => {
  it('tipo "Todos" → tipo: null', () => {
    const s = construirSolicitud('CSV', '', '', 'Todos', [])
    expect(s.tipo).toBeNull()
  })

  it('estados vacío [] → estados: []', () => {
    const s = construirSolicitud('CSV', '', '', 'Todos', [])
    expect(s.estados).toEqual([])
  })

  it('tipo específico se mantiene', () => {
    const s = construirSolicitud('GeoJSON', '2026-10-13', '2026-10-20', 'Bache', [])
    expect(s.tipo).toBe('Bache')
    expect(s.formato).toBe('GeoJSON')
    expect(s.desdeUtc).toBe('2026-10-13T05:00:00Z')
    expect(s.hastaUtc).toBe('2026-10-21T04:59:59Z')
  })
})
