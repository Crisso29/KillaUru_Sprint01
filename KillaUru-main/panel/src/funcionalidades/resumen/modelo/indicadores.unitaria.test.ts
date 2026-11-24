// T069 — indicadores: rótulo, formato de variación, subirEsMalo, null → «—»
import { describe, it, expect } from 'vitest'
import { INDICADORES_META, formatearVariacion, formatearValor, type ClaveIndicador } from './indicadores'

describe('INDICADORES_META', () => {
  it.each<[ClaveIndicador, string, boolean]>([
    ['eventosDia', 'Eventos válidos (ayer)', false],
    ['conductoresActivos', 'Conductores activos', false],
    ['dispositivosDia', 'Dispositivos 24 h', false],
    ['coberturaPct', 'Cobertura', false],
    ['confirmacionPct', 'Confirmación', false],
    ['reaparicionPct', 'Reaparición', true],
  ])('%s → «%s»; subir es malo: %s', (clave, rotulo, subirEsMalo) => {
    // Arrange y Act
    const meta = INDICADORES_META[clave]

    // Assert — solo en Reaparición subir es malo (el color de la variación se invierte)
    expect({ rotulo: meta.rotulo, subirEsMalo: meta.subirEsMalo }).toEqual({ rotulo, subirEsMalo })
  })
})

describe('formatearValor', () => {
  it.each<[number | null, '%' | undefined, string]>([
    [null, undefined, '—'],
    [321, undefined, '321'],
    [71.2, '%', '71,2 %'],
  ])('%s %s → «%s»', (valor, unidad, esperado) => {
    // Arrange y Act
    const texto = formatearValor(valor, unidad)

    // Assert
    expect(texto).toBe(esperado)
  })
})

describe('formatearVariacion', () => {
  it.each<[ClaveIndicador, number | null, string]>([
    ['eventosDia', 8.1, '+8,1 %'],
    ['eventosDia', -3.5, '−3,5 %'],
    ['conductoresActivos', 12, '+12'],
    ['dispositivosDia', -4, '−4'],
    ['coberturaPct', 4, '+4 pts'],
    ['confirmacionPct', 1, '+1 pt'],
    ['confirmacionPct', -1, '−1 pt'],
    ['reaparicionPct', 2, '+2 pts'],
    ['eventosDia', null, '—'],
  ])('%s con variación %s → «%s»', (clave, variacion, esperado) => {
    // Arrange y Act
    const texto = formatearVariacion(clave, variacion)

    // Assert
    expect(texto).toBe(esperado)
  })
})
