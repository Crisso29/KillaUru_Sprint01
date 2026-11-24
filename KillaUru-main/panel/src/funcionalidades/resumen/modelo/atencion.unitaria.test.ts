// T071 — atencion: contarTemas y primerPendiente (data-model §5, FR-018)
import { describe, it, expect } from 'vitest'
import { contarTemas, primerPendiente } from './atencion'
import type { ElementoPendiente } from '@/api/dtos/003-anomalias'

function revision(venceUtc: string) {
  return {
    ultimaRevisionUtc: '2026-10-01T00:00:00Z',
    venceUtc,
    administradores: [],
  }
}

function pendiente(id: string, venceUtc: string): ElementoPendiente {
  return {
    id,
    tipo: 'Bache',
    kmVia: 280.0,
    severidadMostrada: 3,
    reportes: 2,
    primerReporteUtc: '2026-11-01T00:00:00Z',
    venceUtc,
  }
}

const AHORA = new Date('2026-11-07T12:00:00Z')

describe('contarTemas', () => {
  it.each([
    ['por revisar + pendientes, revisión a 16 días', 2, 2, '2026-11-24T00:00:00Z', 4],
    ['la revisión a 15 días o menos suma un tema', 2, 2, '2026-11-22T12:00:00Z', 5],
    ['la revisión vencida también suma un tema', 1, 1, '2026-11-06T12:00:00Z', 3],
    ['nada pendiente y revisión lejana → 0', 0, 0, '2027-01-01T00:00:00Z', 0],
  ])('%s', (_caso, porRevisar, pendientes, venceRevision, esperado) => {
    // Arrange
    const estadoRevision = revision(venceRevision)

    // Act
    const temas = contarTemas(porRevisar, pendientes, estadoRevision, AHORA)

    // Assert
    expect(temas).toBe(esperado)
  })
})

describe('primerPendiente', () => {
  it('lista vacía → null', () => {
    // Arrange y Act
    const primero = primerPendiente([], AHORA)

    // Assert
    expect(primero).toBeNull()
  })

  it('devuelve el id del primero y las horas que faltan', () => {
    // Arrange
    const vence = new Date(AHORA.getTime() + 3 * 60 * 60 * 1000).toISOString()

    // Act
    const resultado = primerPendiente([pendiente('abc12345', vence)], AHORA)

    // Assert
    expect(resultado).toContain('abc12345')
    expect(resultado).toContain('3 h')
  })

  it('usa los primeros 8 caracteres del id', () => {
    // Arrange
    const vence = new Date(AHORA.getTime() + 2 * 60 * 60 * 1000).toISOString()

    // Act
    const resultado = primerPendiente([pendiente('a1b2c3d4-9999', vence)], AHORA)

    // Assert
    expect(resultado).toContain('a1b2c3d4')
  })
})

// Prototipo «A-1061 expira en 18 h»: la que vence antes y nunca horas negativas.
describe('primerPendiente: la más urgente', () => {
  it('elige la que vence antes, no la primera de la lista', () => {
    // Arrange
    const horasDesde = (numero: number) => new Date(AHORA.getTime() + numero * 3600_000).toISOString()
    const lista = [pendiente('tardia00', horasDesde(40)), pendiente('urgente0', horasDesde(5))]

    // Act
    const resultado = primerPendiente(lista, AHORA)

    // Assert
    expect(resultado).toBe('urgente0 expira en 5 h')
  })

  it('si el plazo ya pasó no muestra horas negativas', () => {
    // Arrange
    const vencida = new Date(AHORA.getTime() - 320 * 3600_000).toISOString()

    // Act
    const resultado = primerPendiente([pendiente('vieja000', vencida)], AHORA)

    // Assert
    expect(resultado).toBe('vieja000 expira en menos de 1 h')
  })
})
