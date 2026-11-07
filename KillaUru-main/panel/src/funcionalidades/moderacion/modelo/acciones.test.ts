// T063 — acciones: tabla RN-07; MantenerDescartada solo con porRevisar; texto noDisponible
import { describe, it, expect } from 'vitest'
import { accionesDisponibles, esAccionDisponible, textoNoDisponible } from './acciones'
import type { EstadoAnomalia } from '@/api/generado/003-anomalias'

describe('accionesDisponibles (RN-07)', () => {
  it('Activa → MarcarReparada, Descartar', () => {
    const a = accionesDisponibles('Activa', false)
    expect(a).toContain('MarcarReparada')
    expect(a).toContain('Descartar')
    expect(a).not.toContain('Reabrir')
    expect(a).not.toContain('MantenerDescartada')
  })

  it('PendienteConfirmacion → MarcarReparada, Descartar', () => {
    const a = accionesDisponibles('PendienteConfirmacion', false)
    expect(a).toContain('MarcarReparada')
    expect(a).toContain('Descartar')
    expect(a).not.toContain('Reabrir')
    expect(a).not.toContain('MantenerDescartada')
  })

  it('Expirada → solo Descartar', () => {
    const a = accionesDisponibles('Expirada', false)
    expect(a).not.toContain('MarcarReparada')
    expect(a).toContain('Descartar')
    expect(a).not.toContain('Reabrir')
    expect(a).not.toContain('MantenerDescartada')
  })

  it('Reparada → solo Reabrir', () => {
    const a = accionesDisponibles('Reparada', false)
    expect(a).not.toContain('MarcarReparada')
    expect(a).not.toContain('Descartar')
    expect(a).toContain('Reabrir')
    expect(a).not.toContain('MantenerDescartada')
  })

  it('Descartada sin porRevisar → solo Reabrir', () => {
    const a = accionesDisponibles('Descartada', false)
    expect(a).not.toContain('MarcarReparada')
    expect(a).not.toContain('Descartar')
    expect(a).toContain('Reabrir')
    expect(a).not.toContain('MantenerDescartada')
  })

  it('Descartada con porRevisar → Reabrir y MantenerDescartada', () => {
    const a = accionesDisponibles('Descartada', true)
    expect(a).toContain('Reabrir')
    expect(a).toContain('MantenerDescartada')
  })
})

describe('esAccionDisponible', () => {
  it('MarcarReparada desde Activa → disponible', () => {
    expect(esAccionDisponible('MarcarReparada', 'Activa', false)).toBe(true)
  })

  it('MarcarReparada desde Reparada → no disponible', () => {
    expect(esAccionDisponible('MarcarReparada', 'Reparada', false)).toBe(false)
  })

  it('MantenerDescartada desde Descartada sin porRevisar → no disponible', () => {
    expect(esAccionDisponible('MantenerDescartada', 'Descartada', false)).toBe(false)
  })

  it('MantenerDescartada desde Descartada con porRevisar → disponible', () => {
    expect(esAccionDisponible('MantenerDescartada', 'Descartada', true)).toBe(true)
  })
})

describe('textoNoDisponible', () => {
  it('MarcarReparada desde Reparada → texto con estado actual y estados válidos', () => {
    const texto = textoNoDisponible('MarcarReparada', 'Reparada')
    expect(texto).toMatch(/no disponible desde reparada/i)
    expect(texto).toMatch(/activa/i)
    expect(texto).toMatch(/pendienteconfirmacion/i)
  })

  it('Descartar desde Reparada → incluye estados donde sí está disponible', () => {
    const texto = textoNoDisponible('Descartar', 'Reparada')
    expect(texto).toMatch(/activa/i)
  })

  const casos: [Parameters<typeof textoNoDisponible>[0], EstadoAnomalia][] = [
    ['MarcarReparada', 'Descartada'],
    ['Descartar', 'Reparada'],
    ['Reabrir', 'Activa'],
  ]
  it.each(casos)('devuelve texto no vacío para %s desde %s', (accion, estado) => {
    const texto = textoNoDisponible(accion, estado)
    expect(texto.length).toBeGreaterThan(0)
  })
})
