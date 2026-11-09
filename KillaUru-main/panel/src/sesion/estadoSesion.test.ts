import { describe, it, expect } from 'vitest'
import { reducirSesion, type AccionSesion } from './estadoSesion'
import type { EstadoSesion } from './estadoSesion'

// T015 — transiciones data-model.md §1

const DESCONOCIDA: EstadoSesion = { tipo: 'desconocida' }
const ANONIMA: EstadoSesion = { tipo: 'anonima' }
const ACTIVA: EstadoSesion = {
  tipo: 'activa',
  expiraAccesoUtc: new Date('2026-11-05T10:05:00Z'),
  sesionVenceUtc: new Date('2026-11-05T18:00:00Z'),
  desfaseMs: 240_000,
}

describe('reducirSesion', () => {
  it('desconocida + renovar200 → activa', () => {
    const acc: AccionSesion = {
      tipo: 'renovar_ok',
      expiraAccesoUtc: new Date('2026-11-05T10:05:00Z'),
      sesionVenceUtc: new Date('2026-11-05T18:00:00Z'),
      desfaseMs: 240_000,
    }
    expect(reducirSesion(DESCONOCIDA, acc).tipo).toBe('activa')
  })

  it('desconocida + renovar401 → anonima', () => {
    const acc: AccionSesion = { tipo: 'renovar_401' }
    expect(reducirSesion(DESCONOCIDA, acc).tipo).toBe('anonima')
  })

  it('anonima + login200 → activa', () => {
    const acc: AccionSesion = {
      tipo: 'login_ok',
      expiraAccesoUtc: new Date('2026-11-05T10:05:00Z'),
      sesionVenceUtc: new Date('2026-11-05T18:00:00Z'),
      desfaseMs: 240_000,
    }
    expect(reducirSesion(ANONIMA, acc).tipo).toBe('activa')
  })

  it('activa + renovar200 → activa con nuevos tiempos', () => {
    const nueva = new Date('2026-11-05T10:10:00Z')
    const acc: AccionSesion = {
      tipo: 'renovar_ok',
      expiraAccesoUtc: nueva,
      sesionVenceUtc: ACTIVA.sesionVenceUtc,
      desfaseMs: 300_000,
    }
    const siguiente = reducirSesion(ACTIVA, acc)
    expect(siguiente.tipo).toBe('activa')
    if (siguiente.tipo === 'activa') {
      expect(siguiente.expiraAccesoUtc).toEqual(nueva)
    }
  })

  it('activa + renovar401 → anonima(sesion)', () => {
    const acc: AccionSesion = { tipo: 'renovar_401' }
    const siguiente = reducirSesion(ACTIVA, acc)
    expect(siguiente.tipo).toBe('anonima')
    if (siguiente.tipo === 'anonima') {
      expect(siguiente.motivo).toBe('sesion')
    }
  })

  it('activa + salir → anonima(salida)', () => {
    const acc: AccionSesion = { tipo: 'salir' }
    const siguiente = reducirSesion(ACTIVA, acc)
    expect(siguiente.tipo).toBe('anonima')
    if (siguiente.tipo === 'anonima') {
      expect(siguiente.motivo).toBe('salida')
    }
  })

  it('activa + correo_recibido → activa con correo', () => {
    const acc: AccionSesion = { tipo: 'correo_ok', correo: 'ops@killauru.pe' }
    const siguiente = reducirSesion(ACTIVA, acc)
    if (siguiente.tipo === 'activa') {
      expect(siguiente.correo).toBe('ops@killauru.pe')
    }
  })
})
