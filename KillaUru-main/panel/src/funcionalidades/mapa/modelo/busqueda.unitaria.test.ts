import { describe, it, expect } from 'vitest'
import { parsearBusqueda, masProxima } from './busqueda'
import type { AnomaliaEnMapa } from './busqueda'

// T039 — busqueda: UUID, prefijo 8, km, correo, sin_resultados; masProxima

const UUID = 'a1b2c3d4-0000-0000-0000-000000000001'

describe('parsearBusqueda', () => {
  it('UUID completo → tipo id', () => {
    const r = parsearBusqueda(UUID)
    expect(r.tipo).toBe('id')
    if (r.tipo === 'id') expect(r.id).toBe(UUID)
  })

  it('prefijo de 8 caracteres hex → tipo id-prefijo', () => {
    const r = parsearBusqueda('a1b2c3d4')
    expect(r.tipo).toBe('id-prefijo')
    if (r.tipo === 'id-prefijo') expect(r.prefijo).toBe('a1b2c3d4')
  })

  it('«287,4» → tipo km con km=287.4', () => {
    const r = parsearBusqueda('287,4')
    expect(r.tipo).toBe('km')
    if (r.tipo === 'km') expect(r.km).toBe(287.4)
  })

  it('«10,5» → tipo km', () => {
    const r = parsearBusqueda('10,5')
    expect(r.tipo).toBe('km')
  })

  it('correo con @ → tipo correo', () => {
    const r = parsearBusqueda('ops@killauru.pe')
    expect(r.tipo).toBe('correo')
    if (r.tipo === 'correo') expect(r.correo).toBe('ops@killauru.pe')
  })

  it('texto suelto → sin_resultados', () => {
    expect(parsearBusqueda('texto inventado').tipo).toBe('sin_resultados')
  })

  it('cadena vacía → sin_resultados', () => {
    expect(parsearBusqueda('').tipo).toBe('sin_resultados')
  })
})

describe('masProxima', () => {
  const anomalias: AnomaliaEnMapa[] = [
    { id: '001', kmVia: 285 },
    { id: '002', kmVia: 290 },
    { id: '003', kmVia: 300 },
  ]

  it('287,4 → km 285 (distancia 2,4 vs 2,6)', () => {
    const r = masProxima(287.4, anomalias)
    expect(r?.id).toBe('001')
  })

  it('exactamente en km 290', () => {
    expect(masProxima(290, anomalias)?.id).toBe('002')
  })

  it('lista vacía → null', () => {
    expect(masProxima(287.4, [])).toBeNull()
  })
})
