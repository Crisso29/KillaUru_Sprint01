// T015 — planificador: desfase, renovar a 61/60/59 s, fracción acotada a [0,1]
import { describe, it, expect } from 'vitest'
import { calcularDesfase, debeRenovar, fraccionRestante } from './planificador'
import { MARGEN_RENOVACION_S, SESION_MAXIMA_HORAS } from '@/config/constantes'

const HORA = 3600_000

describe('calcularDesfase', () => {
  it('devuelve (expiraAccesoUtc − VIDA_ACCESO) − ahoraLocal en ms', () => {
    // Arrange — el token vence a las 10:05 y vive 5 min: se emitió a las 10:00, la misma hora del reloj local
    const expiraUtc = new Date('2026-11-05T10:05:00Z')
    const ahora = new Date('2026-11-05T10:00:00Z')

    // Act
    const desfase = calcularDesfase(expiraUtc, ahora)

    // Assert
    expect(desfase).toBe(0)
  })
})

describe('debeRenovar (renueva cuando falta el margen o menos)', () => {
  const expira = new Date('2026-11-05T10:05:00Z')

  it.each([
    ['61 s antes → NO renueva', 61, false],
    [`${MARGEN_RENOVACION_S} s antes (el margen) → SÍ renueva`, MARGEN_RENOVACION_S, true],
    ['59 s antes → SÍ renueva', 59, true],
    ['justo vencido → SÍ renueva', 0, true],
  ])('%s', (_caso, segundosAntes, esperado) => {
    // Arrange
    const ahora = new Date(expira.getTime() - segundosAntes * 1000)

    // Act
    const renovar = debeRenovar(expira, ahora)

    // Assert
    expect(renovar).toBe(esperado)
  })
})

describe('fraccionRestante (barra de la sesión, acotada a [0, 1])', () => {
  const BASE = new Date('2026-11-05T10:00:00Z')

  it.each([
    ['a mitad de la sesión → 0,5', (SESION_MAXIMA_HORAS * HORA) / 2, 0.5],
    ['sesión ya vencida → 0', -1000, 0],
    ['más larga que el máximo → 1', 100 * HORA, 1],
  ])('%s', (_caso, msHastaVencer, esperado) => {
    // Arrange
    const sesionVence = new Date(BASE.getTime() + msHastaVencer)

    // Act
    const fraccion = fraccionRestante(sesionVence, BASE)

    // Assert
    expect(fraccion).toBeCloseTo(esperado)
  })
})
