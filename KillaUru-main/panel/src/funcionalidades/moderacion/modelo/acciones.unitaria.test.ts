// T063 — acciones: tabla RN-07; MantenerDescartada solo con porRevisar; texto noDisponible
import { describe, it, expect } from 'vitest'
import { accionesDisponibles, esAccionDisponible, textoNoDisponible } from './acciones'
import type { EstadoAnomalia } from '@/api/dtos/003-anomalias'

type Accion = Parameters<typeof textoNoDisponible>[0]

describe('accionesDisponibles (RN-07)', () => {
  it.each<[EstadoAnomalia, boolean, Accion[]]>([
    ['Activa', false, ['MarcarReparada', 'Descartar']],
    ['PendienteConfirmacion', false, ['MarcarReparada', 'Descartar']],
    ['Expirada', false, ['Descartar']],
    ['Reparada', false, ['Reabrir']],
    ['Descartada', false, ['Reabrir']],
    ['Descartada', true, ['Reabrir', 'MantenerDescartada']], // solo si está «por revisar»
  ])('%s (por revisar: %s) → exactamente %j', (estado, porRevisar, esperadas) => {
    // Arrange y Act
    const acciones = accionesDisponibles(estado, porRevisar)

    // Assert — el conjunto exacto: ni falta una acción ni sobra otra
    expect([...acciones].sort()).toEqual([...esperadas].sort())
  })
})

describe('esAccionDisponible', () => {
  it.each<[Accion, EstadoAnomalia, boolean, boolean]>([
    ['MarcarReparada', 'Activa', false, true],
    ['MarcarReparada', 'Reparada', false, false],
    ['MantenerDescartada', 'Descartada', false, false],
    ['MantenerDescartada', 'Descartada', true, true],
  ])('%s desde %s (por revisar: %s) → %s', (accion, estado, porRevisar, esperado) => {
    // Arrange y Act
    const disponible = esAccionDisponible(accion, estado, porRevisar)

    // Assert
    expect(disponible).toBe(esperado)
  })
})

describe('textoNoDisponible', () => {
  it('MarcarReparada desde Reparada → texto con estado actual y estados válidos', () => {
    // Arrange y Act
    const texto = textoNoDisponible('MarcarReparada', 'Reparada')

    // Assert
    expect(texto).toMatch(/no disponible desde reparada/i)
    expect(texto).toMatch(/activa/i)
    expect(texto).toMatch(/pendienteconfirmacion/i)
  })

  it('Descartar desde Reparada → incluye estados donde sí está disponible', () => {
    // Arrange y Act
    const texto = textoNoDisponible('Descartar', 'Reparada')

    // Assert
    expect(texto).toMatch(/activa/i)
  })

  it.each<[Accion, EstadoAnomalia]>([
    ['MarcarReparada', 'Descartada'],
    ['Descartar', 'Reparada'],
    ['Reabrir', 'Activa'],
  ])('devuelve texto no vacío para %s desde %s', (accion, estado) => {
    // Arrange y Act
    const texto = textoNoDisponible(accion, estado)

    // Assert
    expect(texto.length).toBeGreaterThan(0)
  })
})
