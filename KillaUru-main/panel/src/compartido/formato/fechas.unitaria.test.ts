// T014 — fechas en hora de Lima (UTC−5, sin horario de verano) y en UTC
import { describe, it, expect } from 'vitest'
import { formatearDiaLargoLima, formatearFechaLima, formatearHoraLima, formatearUtcCorto } from './fechas'

describe('formatearFechaLima', () => {
  it.each([
    ['2026-10-21T04:59:00Z', '20 oct'], // 23:59 del 20 en Lima
    ['2026-10-21T05:00:00Z', '21 oct'], // 00:00 del 21 en Lima
    ['2026-11-05T12:00:00Z', '5 nov'],
  ])('%s → «%s»', (iso, esperado) => {
    // Arrange y Act
    const fecha = formatearFechaLima(iso)

    // Assert
    expect(fecha).toBe(esperado)
  })
})

describe('formatearHoraLima', () => {
  it('20:00 UTC → 15:00 en Lima', () => {
    // Arrange y Act
    const hora = formatearHoraLima('2026-11-05T20:00:00Z')

    // Assert
    expect(hora).toBe('15:00')
  })
})

// Prototipo, subtítulo del Resumen: «Lunes 20 de octubre · hora de Lima».
describe('formatearDiaLargoLima', () => {
  it.each([
    ['día de la semana con mayúscula, día y mes', '2026-10-20T15:00:00Z', 'Martes 20 de octubre'],
    ['las 02:00 UTC todavía son el día anterior en Lima', '2026-11-23T02:00:00Z', 'Domingo 22 de noviembre'],
  ])('%s', (_caso, iso, esperado) => {
    // Arrange y Act
    const dia = formatearDiaLargoLima(iso)

    // Assert
    expect(dia).toBe(esperado)
  })
})

describe('formatearUtcCorto', () => {
  it('escribe día, mes abreviado y hora en UTC', () => {
    // Arrange y Act
    const texto = formatearUtcCorto('2026-10-20T10:12:00Z')

    // Assert
    expect(texto).toBe('20 oct 10:12 UTC')
  })
})
