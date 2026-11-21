import { describe, it, expect } from 'vitest'
import {
  filtrosDefecto,
  filtrosDeParams,
  filtrosAParams,
  esGrupoVacio,
} from './filtros'

// T035 — FiltrosMapa: defecto, ida/vuelta URL, desconocidos descartados, grupo vacío

describe('filtrosDefecto', () => {
  it('devuelve Activa+PendienteConfirmacion, ambos tipos, 1-5, 7d', () => {
    const f = filtrosDefecto()
    expect(f.estados).toEqual(['Activa', 'PendienteConfirmacion'])
    expect(f.tipos).toEqual(['Bache', 'Hundimiento'])
    expect(f.severidades).toEqual([1, 2, 3, 4, 5])
    expect(f.periodo).toBe('7d')
  })
})

describe('filtrosDeParams (sin parámetros → defecto)', () => {
  it('URLSearchParams vacío → filtros defecto', () => {
    const f = filtrosDeParams(new URLSearchParams())
    expect(f).toEqual(filtrosDefecto())
  })
})

describe('ida y vuelta con la dirección', () => {
  it('defecto → params → vuelve igual', () => {
    const original = filtrosDefecto()
    const vuelta = filtrosDeParams(filtrosAParams(original))
    expect(vuelta).toEqual(original)
  })

  it('un solo estado se conserva', () => {
    const f = { ...filtrosDefecto(), estados: ['Activa'] as const }
    const vuelta = filtrosDeParams(filtrosAParams(f))
    expect(vuelta.estados).toEqual(['Activa'])
  })

  it('periodo 24h se conserva', () => {
    const f = { ...filtrosDefecto(), periodo: '24h' as const }
    const vuelta = filtrosDeParams(filtrosAParams(f))
    expect(vuelta.periodo).toBe('24h')
  })

  it('severidades parciales se conservan', () => {
    const f = { ...filtrosDefecto(), severidades: [3, 4, 5] }
    const vuelta = filtrosDeParams(filtrosAParams(f))
    expect(vuelta.severidades).toEqual([3, 4, 5])
  })
})

describe('valores desconocidos descartados', () => {
  it('estado desconocido se filtra', () => {
    const p = new URLSearchParams('estado=Activa,EstadoInventado')
    const f = filtrosDeParams(p)
    expect(f.estados).toEqual(['Activa'])
    expect(f.estados).not.toContain('EstadoInventado')
  })

  it('tipo desconocido se filtra', () => {
    const p = new URLSearchParams('tipo=Bache,TipoFalso')
    const f = filtrosDeParams(p)
    expect(f.tipos).toEqual(['Bache'])
  })

  it('severidad fuera de rango se filtra', () => {
    const p = new URLSearchParams('severidad=2,6,0')
    const f = filtrosDeParams(p)
    expect(f.severidades).toEqual([2])
  })

  it('periodo desconocido cae en 7d', () => {
    const p = new URLSearchParams('periodo=1mes')
    const f = filtrosDeParams(p)
    expect(f.periodo).toBe('7d')
  })
})

describe('esGrupoVacio', () => {
  it('sin estados → vacío', () => {
    expect(esGrupoVacio({ ...filtrosDefecto(), estados: [] })).toBe(true)
  })

  it('sin tipos → vacío', () => {
    expect(esGrupoVacio({ ...filtrosDefecto(), tipos: [] })).toBe(true)
  })

  it('sin severidades → vacío', () => {
    expect(esGrupoVacio({ ...filtrosDefecto(), severidades: [] })).toBe(true)
  })

  it('filtros defecto → no vacío', () => {
    expect(esGrupoVacio(filtrosDefecto())).toBe(false)
  })
})
