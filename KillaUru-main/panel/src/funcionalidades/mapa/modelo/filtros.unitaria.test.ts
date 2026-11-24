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
    // Arrange y Act
    const filtros = filtrosDefecto()

    // Assert
    expect(filtros.estados).toEqual(['Activa', 'PendienteConfirmacion'])
    expect(filtros.tipos).toEqual(['Bache', 'Hundimiento'])
    expect(filtros.severidades).toEqual([1, 2, 3, 4, 5])
    expect(filtros.periodo).toBe('7d')
  })
})

describe('filtrosDeParams (sin parámetros → defecto)', () => {
  it('URLSearchParams vacío → filtros defecto', () => {
    // Arrange y Act
    const filtros = filtrosDeParams(new URLSearchParams())

    // Assert
    expect(filtros).toEqual(filtrosDefecto())
  })
})

describe('ida y vuelta con la dirección', () => {
  it('defecto → params → vuelve igual', () => {
    // Arrange
    const original = filtrosDefecto()

    // Act
    const vuelta = filtrosDeParams(filtrosAParams(original))

    // Assert
    expect(vuelta).toEqual(original)
  })

  it('un solo estado se conserva', () => {
    // Arrange
    const filtros = { ...filtrosDefecto(), estados: ['Activa'] as const }

    // Act
    const vuelta = filtrosDeParams(filtrosAParams(filtros))

    // Assert
    expect(vuelta.estados).toEqual(['Activa'])
  })

  it('periodo 24h se conserva', () => {
    // Arrange
    const filtros = { ...filtrosDefecto(), periodo: '24h' as const }

    // Act
    const vuelta = filtrosDeParams(filtrosAParams(filtros))

    // Assert
    expect(vuelta.periodo).toBe('24h')
  })

  it('severidades parciales se conservan', () => {
    // Arrange
    const filtros = { ...filtrosDefecto(), severidades: [3, 4, 5] }

    // Act
    const vuelta = filtrosDeParams(filtrosAParams(filtros))

    // Assert
    expect(vuelta.severidades).toEqual([3, 4, 5])
  })
})

describe('valores desconocidos descartados', () => {
  it('estado desconocido se filtra', () => {
    // Arrange
    const parametros = new URLSearchParams('estado=Activa,EstadoInventado')

    // Act
    const filtros = filtrosDeParams(parametros)

    // Assert
    expect(filtros.estados).toEqual(['Activa'])
    expect(filtros.estados).not.toContain('EstadoInventado')
  })

  it('tipo desconocido se filtra', () => {
    // Arrange
    const parametros = new URLSearchParams('tipo=Bache,TipoFalso')

    // Act
    const filtros = filtrosDeParams(parametros)

    // Assert
    expect(filtros.tipos).toEqual(['Bache'])
  })

  it('severidad fuera de rango se filtra', () => {
    // Arrange
    const parametros = new URLSearchParams('severidad=2,6,0')

    // Act
    const filtros = filtrosDeParams(parametros)

    // Assert
    expect(filtros.severidades).toEqual([2])
  })

  it('periodo desconocido cae en 7d', () => {
    // Arrange
    const parametros = new URLSearchParams('periodo=1mes')

    // Act
    const filtros = filtrosDeParams(parametros)

    // Assert
    expect(filtros.periodo).toBe('7d')
  })
})

describe('esGrupoVacio', () => {
  it.each([
    ['sin estados', { estados: [] }, true],
    ['sin tipos', { tipos: [] }, true],
    ['sin severidades', { severidades: [] }, true],
    ['filtros por defecto', {}, false],
  ])('%s → vacío: %s', (_caso, cambio, esperado) => {
    // Arrange
    const filtros = { ...filtrosDefecto(), ...cambio }

    // Act
    const vacio = esGrupoVacio(filtros)

    // Assert
    expect(vacio).toBe(esperado)
  })
})
