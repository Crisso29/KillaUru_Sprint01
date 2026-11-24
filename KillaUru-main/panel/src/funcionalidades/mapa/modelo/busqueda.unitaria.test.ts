// T039 — busqueda: UUID, prefijo 8, km, correo, sin_resultados; masProxima
import { describe, it, expect } from 'vitest'
import { parsearBusqueda, masProxima, type AnomaliaEnMapa, type IntentoBusqueda } from './busqueda'

const UUID = 'a1b2c3d4-0000-0000-0000-000000000001'

describe('parsearBusqueda', () => {
  it.each<[string, IntentoBusqueda]>([
    [UUID, { tipo: 'id', id: UUID }],
    ['a1b2c3d4', { tipo: 'id-prefijo', prefijo: 'a1b2c3d4' }], // el id corto que se ve en las tablas
    ['287,4', { tipo: 'km', km: 287.4 }], // coma decimal, como se escribe en Perú
    ['10,5', { tipo: 'km', km: 10.5 }],
    ['ops@killauru.pe', { tipo: 'correo', correo: 'ops@killauru.pe' }],
    ['texto inventado', { tipo: 'sin_resultados' }],
    ['', { tipo: 'sin_resultados' }],
  ])('«%s» → %j', (texto, esperado) => {
    // Arrange y Act
    const intento = parsearBusqueda(texto)

    // Assert
    expect(intento).toEqual(esperado)
  })
})

describe('masProxima', () => {
  const anomalias: AnomaliaEnMapa[] = [
    { id: '001', kmVia: 285 },
    { id: '002', kmVia: 290 },
    { id: '003', kmVia: 300 },
  ]

  it.each([
    ['287,4 → km 285 (distancia 2,4 contra 2,6)', 287.4, anomalias, '001'],
    ['exactamente en km 290', 290, anomalias, '002'],
    ['lista vacía → ninguna', 287.4, [], undefined],
  ])('%s', (_caso, km, lista, esperado) => {
    // Arrange y Act
    const anomalia = masProxima(km, lista)

    // Assert
    expect(anomalia?.id).toBe(esperado)
  })
})
