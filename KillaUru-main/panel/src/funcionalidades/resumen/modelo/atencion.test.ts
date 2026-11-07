// T071 — atencion: contarTemas y primerPendiente (data-model §5, FR-018)
import { describe, it, expect } from 'vitest'
import { contarTemas, primerPendiente } from './atencion'
import type { ElementoPendiente } from '@/api/generado/003-anomalias'

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
  it('por revisar + pendientes sin revisión próxima', () => {
    const r = revision('2026-11-24T00:00:00Z') // 16 días exactos
    expect(contarTemas(2, 2, r, AHORA)).toBe(4)
  })

  it('revisión a 15 días suma 1 más', () => {
    const r = revision('2026-11-22T12:00:00Z') // 15 días exactos
    expect(contarTemas(2, 2, r, AHORA)).toBe(5)
  })

  it('revisión vencida suma 1 más', () => {
    const r = revision('2026-11-06T12:00:00Z') // ayer
    expect(contarTemas(1, 1, r, AHORA)).toBe(3)
  })

  it('cero por revisar y cero pendientes, revisión lejana → 0', () => {
    const r = revision('2027-01-01T00:00:00Z')
    expect(contarTemas(0, 0, r, AHORA)).toBe(0)
  })
})

describe('primerPendiente', () => {
  it('lista vacía → null', () => {
    expect(primerPendiente([], AHORA)).toBeNull()
  })

  it('devuelve el id del primero y las horas que faltan', () => {
    const vence = new Date(AHORA.getTime() + 3 * 60 * 60 * 1000).toISOString()
    const resultado = primerPendiente([pendiente('abc12345', vence)], AHORA)
    expect(resultado).toContain('abc12345')
    expect(resultado).toContain('3 h')
  })

  it('usa los primeros 8 caracteres del id', () => {
    const vence = new Date(AHORA.getTime() + 2 * 60 * 60 * 1000).toISOString()
    const resultado = primerPendiente([pendiente('a1b2c3d4-9999', vence)], AHORA)
    expect(resultado).toContain('a1b2c3d4')
  })
})
