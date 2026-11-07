// T072 — textoQuePaso: tabla completa de data-model §5 (FR-020, R-014)
import { describe, it, expect } from 'vitest'
import { textoQuePaso } from './transicion'

describe('textoQuePaso', () => {
  it('null → Activa sin actor → «Creada con el primer reporte»', () => {
    expect(textoQuePaso(null, 'Activa', null)).toBe('Creada con el primer reporte')
  })

  it('null → PendienteConfirmacion sin actor → «Nueva: espera confirmación»', () => {
    expect(textoQuePaso(null, 'PendienteConfirmacion', null)).toBe('Nueva: espera confirmación')
  })

  it('PendienteConfirmacion → Activa sin actor → «Confirmada por 2.º vehículo»', () => {
    expect(textoQuePaso('PendienteConfirmacion', 'Activa', null)).toBe('Confirmada por 2.º vehículo')
  })

  it('Activa → Expirada sin actor → «Expiró sin reportes nuevos»', () => {
    expect(textoQuePaso('Activa', 'Expirada', null)).toBe('Expiró sin reportes nuevos')
  })

  it('PendienteConfirmacion → Expirada sin actor → «Expiró sin reportes nuevos»', () => {
    expect(textoQuePaso('PendienteConfirmacion', 'Expirada', null)).toBe('Expiró sin reportes nuevos')
  })

  it('Expirada → Activa sin actor → «Volvió con un reporte nuevo»', () => {
    expect(textoQuePaso('Expirada', 'Activa', null)).toBe('Volvió con un reporte nuevo')
  })

  it('Reparada → Activa sin actor → «Volvió con un reporte nuevo»', () => {
    expect(textoQuePaso('Reparada', 'Activa', null)).toBe('Volvió con un reporte nuevo')
  })

  it('Expirada → PendienteConfirmacion sin actor → «Volvió con un reporte grave: espera confirmación»', () => {
    expect(textoQuePaso('Expirada', 'PendienteConfirmacion', null)).toBe(
      'Volvió con un reporte grave: espera confirmación',
    )
  })

  it('cualquiera → Reparada con actor → «Marcada reparada»', () => {
    expect(textoQuePaso('Activa', 'Reparada', 'actor-id')).toBe('Marcada reparada')
  })

  it('cualquiera → Descartada con actor → «Descartada»', () => {
    expect(textoQuePaso('Activa', 'Descartada', 'actor-id')).toBe('Descartada')
  })

  it('Reparada → Activa con actor → «Reabierta»', () => {
    expect(textoQuePaso('Reparada', 'Activa', 'actor-id')).toBe('Reabierta')
  })

  it('Descartada → Activa con actor → «Reabierta»', () => {
    expect(textoQuePaso('Descartada', 'Activa', 'actor-id')).toBe('Reabierta')
  })

  it('combinación no prevista → «Cambió a <estado>»', () => {
    expect(textoQuePaso('Activa', 'Expirada', 'actor-id')).toBe('Cambió a Expirada')
  })
})
