// T014 — «hace 2 h», «ayer a las 15:30»: tiempo relativo en hora de Lima (UTC−5)
import { describe, it, expect } from 'vitest'
import { formatearRelativo } from './relativo'

// 17:00 UTC = 12:00 del 5 de noviembre en Lima
const AHORA = new Date('2026-11-05T17:00:00Z')
const MINUTO = 60_000
const DIA = 24 * 60 * MINUTO

describe('formatearRelativo', () => {
  it.each([
    ['hace 30 s → «ahora mismo»', new Date(AHORA.getTime() - 30_000).toISOString(), 'ahora mismo'],
    ['mismo día, 2 h antes → «hace 2 h»', '2026-11-05T15:00:00Z', 'hace 2 h'],
    ['ayer en Lima a las 15:30 (20:30 UTC)', '2026-11-04T20:30:00Z', 'ayer a las 15:30'],
    ['las 23:00 de ayer vistas a mediodía siguen siendo «ayer», no «hace 13 h»', '2026-11-05T04:00:00Z', 'ayer a las 23:00'],
    ['hace 3 días', new Date(AHORA.getTime() - 3 * DIA).toISOString(), 'hace 3 d'],
  ])('%s', (_caso, iso, esperado) => {
    // Arrange y Act
    const texto = formatearRelativo(iso, AHORA)

    // Assert
    expect(texto).toBe(esperado)
  })
})
