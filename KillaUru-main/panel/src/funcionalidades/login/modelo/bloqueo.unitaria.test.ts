// T028 — bloqueo: N min redondeado arriba; hora Lima; reactiva exactamente
import { describe, it, expect } from 'vitest'
import { minutosRestantes, horaEnLima, estaDesbloqueado } from './bloqueo'

const AHORA = new Date('2026-11-05T10:00:00Z')
const HASTA = new Date('2026-11-05T10:04:01Z')

describe('minutosRestantes (se redondea hacia arriba: nunca «0 min» con la cuenta bloqueada)', () => {
  it.each([
    ['4 min 01 s', '2026-11-05T10:04:01Z', 5],
    ['exactamente 4 min', '2026-11-05T10:04:00Z', 4],
    ['1 s', '2026-11-05T10:00:01Z', 1],
  ])('%s → %i', (_caso, hastaIso, esperado) => {
    // Arrange
    const hasta = new Date(hastaIso)

    // Act
    const minutos = minutosRestantes(hasta, AHORA)

    // Assert
    expect(minutos).toBe(esperado)
  })
})

describe('horaEnLima (UTC−5)', () => {
  it.each([
    ['2026-11-05T15:30:00Z', '10:30'],
    ['2026-11-05T00:00:00Z', '19:00'], // del día anterior en Lima
    ['2026-11-05T08:05:00Z', '03:05'], // siempre con dos dígitos
  ])('%s → %s', (iso, esperado) => {
    // Arrange
    const fecha = new Date(iso)

    // Act
    const hora = horaEnLima(fecha)

    // Assert
    expect(hora).toBe(esperado)
  })
})

describe('estaDesbloqueado (se reactiva exactamente a la hora indicada)', () => {
  it.each([
    ['un ms antes → bloqueado', '2026-11-05T10:04:00.999Z', false],
    ['exactamente a esa hora → desbloqueado', '2026-11-05T10:04:01Z', true],
    ['después → desbloqueado', '2026-11-05T10:05:00Z', true],
  ])('%s', (_caso, ahoraIso, esperado) => {
    // Arrange
    const ahora = new Date(ahoraIso)

    // Act
    const desbloqueado = estaDesbloqueado(HASTA, ahora)

    // Assert
    expect(desbloqueado).toBe(esperado)
  })
})
