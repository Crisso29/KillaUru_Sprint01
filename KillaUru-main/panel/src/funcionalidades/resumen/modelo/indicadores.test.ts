// T069 — indicadores: rótulo, formato de variación, subirEsMalo, null → «—»
import { describe, it, expect } from 'vitest'
import {
  INDICADORES_META,
  formatearVariacion,
  formatearValor,
} from './indicadores'

describe('INDICADORES_META', () => {
  it('eventosDia → «Eventos válidos (ayer)»', () => {
    expect(INDICADORES_META.eventosDia.rotulo).toBe('Eventos válidos (ayer)')
  })

  it('conductoresActivos → «Conductores activos»', () => {
    expect(INDICADORES_META.conductoresActivos.rotulo).toBe('Conductores activos')
  })

  it('dispositivosDia → «Dispositivos 24 h»', () => {
    expect(INDICADORES_META.dispositivosDia.rotulo).toBe('Dispositivos 24 h')
  })

  it('coberturaPct → «Cobertura»', () => {
    expect(INDICADORES_META.coberturaPct.rotulo).toBe('Cobertura')
  })

  it('confirmacionPct → «Confirmación»', () => {
    expect(INDICADORES_META.confirmacionPct.rotulo).toBe('Confirmación')
  })

  it('reaparicionPct → «Reaparición»', () => {
    expect(INDICADORES_META.reaparicionPct.rotulo).toBe('Reaparición')
  })

  it('reaparicionPct: subirEsMalo = true', () => {
    expect(INDICADORES_META.reaparicionPct.subirEsMalo).toBe(true)
  })

  it('los demás: subirEsMalo = false', () => {
    const otros = ['eventosDia', 'conductoresActivos', 'dispositivosDia', 'coberturaPct', 'confirmacionPct'] as const
    for (const k of otros) {
      expect(INDICADORES_META[k].subirEsMalo).toBe(false)
    }
  })
})

describe('formatearValor', () => {
  it('null → «—»', () => {
    expect(formatearValor(null)).toBe('—')
  })

  it('321 → «321»', () => {
    expect(formatearValor(321)).toBe('321')
  })

  it('71.2 con unidad % → «71,2 %»', () => {
    expect(formatearValor(71.2, '%')).toBe('71,2 %')
  })
})

describe('formatearVariacion', () => {
  it('eventosDia variacion 8.1 → «+8,1 %»', () => {
    expect(formatearVariacion('eventosDia', 8.1)).toBe('+8,1 %')
  })

  it('eventosDia variacion negativa −3.5 → «−3,5 %»', () => {
    expect(formatearVariacion('eventosDia', -3.5)).toBe('−3,5 %')
  })

  it('conductoresActivos variacion 12 → «+12»', () => {
    expect(formatearVariacion('conductoresActivos', 12)).toBe('+12')
  })

  it('dispositivosDia variacion −4 → «−4»', () => {
    expect(formatearVariacion('dispositivosDia', -4)).toBe('−4')
  })

  it('coberturaPct variacion 4 → «+4 pts»', () => {
    expect(formatearVariacion('coberturaPct', 4)).toBe('+4 pts')
  })

  it('confirmacionPct variacion 1 → «+1 pt» (singular)', () => {
    expect(formatearVariacion('confirmacionPct', 1)).toBe('+1 pt')
  })

  it('confirmacionPct variacion −1 → «−1 pt» (singular)', () => {
    expect(formatearVariacion('confirmacionPct', -1)).toBe('−1 pt')
  })

  it('reaparicionPct variacion 2 → «+2 pts»', () => {
    expect(formatearVariacion('reaparicionPct', 2)).toBe('+2 pts')
  })

  it('null → «—»', () => {
    expect(formatearVariacion('eventosDia', null)).toBe('—')
  })
})
