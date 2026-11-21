import { describe, it, expect } from 'vitest'
import { filtrosAConsulta } from './peticion'
import { filtrosDefecto } from './filtros'

// T036 — filtrosAConsulta: estado siempre, tipo solo con uno, severidad omitida con 5, desde fijo

const AHORA = new Date('2026-11-05T12:00:00.000Z')

describe('filtrosAConsulta', () => {
  it('estado siempre presente (dos estados)', () => {
    const p = filtrosAConsulta(filtrosDefecto(), AHORA)
    expect(p.has('estado')).toBe(true)
    expect(p.get('estado')).toBe('Activa,PendienteConfirmacion')
  })

  it('tipo ausente con ambos tipos', () => {
    const p = filtrosAConsulta(filtrosDefecto(), AHORA)
    expect(p.has('tipo')).toBe(false)
  })

  it('tipo presente con un solo tipo', () => {
    const f = { ...filtrosDefecto(), tipos: ['Bache'] as const }
    const p = filtrosAConsulta(f, AHORA)
    expect(p.get('tipo')).toBe('Bache')
  })

  it('severidad_min ausente con las 5', () => {
    const p = filtrosAConsulta(filtrosDefecto(), AHORA)
    expect(p.has('severidad_min')).toBe(false)
  })

  it('severidad_min presente con menos de 5', () => {
    const f = { ...filtrosDefecto(), severidades: [3, 4, 5] }
    const p = filtrosAConsulta(f, AHORA)
    expect(p.get('severidad_min')).toBe('3')
  })

  it('desde = ahora − 24h', () => {
    const f = { ...filtrosDefecto(), periodo: '24h' as const }
    const p = filtrosAConsulta(f, AHORA)
    expect(p.get('desde')).toBe('2026-11-04T12:00:00.000Z')
  })

  it('desde = ahora − 7d', () => {
    const p = filtrosAConsulta(filtrosDefecto(), AHORA)
    expect(p.get('desde')).toBe('2026-10-29T12:00:00.000Z')
  })

  it('desde = ahora − 30d', () => {
    const f = { ...filtrosDefecto(), periodo: '30d' as const }
    const p = filtrosAConsulta(f, AHORA)
    expect(p.get('desde')).toBe('2026-10-06T12:00:00.000Z')
  })

  it('nunca bbox ni updatedSince', () => {
    const p = filtrosAConsulta(filtrosDefecto(), AHORA)
    expect(p.has('bbox')).toBe(false)
    expect(p.has('updatedSince')).toBe(false)
  })
})
