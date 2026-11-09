import { describe, it, expect } from 'vitest'
import { calcularDesfase, debeRenovar, fraccionRestante } from './planificador'
import {
  MARGEN_RENOVACION_S,
  SESION_MAXIMA_HORAS,
} from '@/config/constantes'

// T015 — planificador: desfase, renovar a 61/60/59 s, fracción acotada a [0,1]

describe('calcularDesfase', () => {
  it('devuelve (expiraAccesoUtc − VIDA_ACCESO) − ahoraLocal en ms', () => {
    const expiraUtc = new Date('2026-11-05T10:05:00Z')
    const ahora = new Date('2026-11-05T10:00:00Z') // 5 min antes
    const desfase = calcularDesfase(expiraUtc, ahora)
    // expira - VIDA_ACCESO_MIN (5 min) = 10:00; ahora = 10:00 → desfase = 0
    expect(desfase).toBe(0)
  })
})

describe('debeRenovar', () => {
  const expira = new Date('2026-11-05T10:05:00Z')

  it('61 s antes → NO renueva', () => {
    const ahora = new Date(expira.getTime() - 61_000)
    expect(debeRenovar(expira, ahora)).toBe(false)
  })

  it('60 s antes → SÍ renueva', () => {
    const ahora = new Date(expira.getTime() - MARGEN_RENOVACION_S * 1000)
    expect(debeRenovar(expira, ahora)).toBe(true)
  })

  it('59 s antes → SÍ renueva', () => {
    const ahora = new Date(expira.getTime() - 59_000)
    expect(debeRenovar(expira, ahora)).toBe(true)
  })

  it('justo vencido → SÍ renueva', () => {
    expect(debeRenovar(expira, expira)).toBe(true)
  })
})

describe('fraccionRestante', () => {
  const BASE = new Date('2026-11-05T10:00:00Z')

  it('fracción dentro de [0, 1]', () => {
    const sesionVence = new Date(BASE.getTime() + SESION_MAXIMA_HORAS * 3600_000 / 2)
    const f = fraccionRestante(sesionVence, BASE)
    expect(f).toBeGreaterThan(0)
    expect(f).toBeLessThanOrEqual(1)
  })

  it('sesión ya vencida → 0', () => {
    const sesionVence = new Date(BASE.getTime() - 1000)
    expect(fraccionRestante(sesionVence, BASE)).toBe(0)
  })

  it('sesión muy larga → 1', () => {
    const sesionVence = new Date(BASE.getTime() + 100 * 3600_000)
    expect(fraccionRestante(sesionVence, BASE)).toBe(1)
  })
})
