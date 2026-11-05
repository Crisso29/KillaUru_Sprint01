import { describe, it, expect } from 'vitest'
import { minutosRestantes, horaEnLima, estaDesbloqueado } from './bloqueo'

// T028 — bloqueo: N min redondeado arriba; hora Lima; reactiva exactamente

describe('minutosRestantes', () => {
  it('4 min 01 s → 5', () => {
    const ahora = new Date('2026-11-05T10:00:00Z')
    const hasta = new Date('2026-11-05T10:04:01Z')
    expect(minutosRestantes(hasta, ahora)).toBe(5)
  })

  it('exactamente 4 min → 4', () => {
    const ahora = new Date('2026-11-05T10:00:00Z')
    const hasta = new Date('2026-11-05T10:04:00Z')
    expect(minutosRestantes(hasta, ahora)).toBe(4)
  })

  it('1 s → 1', () => {
    const ahora = new Date('2026-11-05T10:00:00Z')
    const hasta = new Date('2026-11-05T10:00:01Z')
    expect(minutosRestantes(hasta, ahora)).toBe(1)
  })
})

describe('horaEnLima', () => {
  it('15:30 UTC → 10:30 en Lima (UTC-5)', () => {
    const d = new Date('2026-11-05T15:30:00Z')
    expect(horaEnLima(d)).toBe('10:30')
  })

  it('00:00 UTC → 19:00 del día anterior en Lima', () => {
    const d = new Date('2026-11-05T00:00:00Z')
    expect(horaEnLima(d)).toBe('19:00')
  })

  it('formatea con dos dígitos', () => {
    const d = new Date('2026-11-05T08:05:00Z')
    expect(horaEnLima(d)).toBe('03:05')
  })
})

describe('estaDesbloqueado', () => {
  it('un ms antes → bloqueado', () => {
    const hasta = new Date('2026-11-05T10:04:01Z')
    const antes = new Date('2026-11-05T10:04:00.999Z')
    expect(estaDesbloqueado(hasta, antes)).toBe(false)
  })

  it('exactamente a esa hora → desbloqueado', () => {
    const hasta = new Date('2026-11-05T10:04:01Z')
    const igual = new Date('2026-11-05T10:04:01Z')
    expect(estaDesbloqueado(hasta, igual)).toBe(true)
  })

  it('después → desbloqueado', () => {
    const hasta = new Date('2026-11-05T10:04:01Z')
    const despues = new Date('2026-11-05T10:05:00Z')
    expect(estaDesbloqueado(hasta, despues)).toBe(true)
  })
})
