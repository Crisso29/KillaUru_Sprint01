// T082 — nombreArchivo: genera nombre de archivo por formato
import { describe, it, expect } from 'vitest'
import { nombreArchivo } from './descarga'

describe('nombreArchivo', () => {
  it.each([
    ['CSV', '2026-10-13', '2026-10-20', 'killauru-anomalias-20261013-20261020.csv'],
    ['GeoJSON', '2026-10-13', '2026-10-20', 'killauru-anomalias-20261013-20261020.geojson'],
    ['CSV', '', '', 'killauru-anomalias.csv'], // sin fechas: solo el prefijo
  ] as const)('%s del %s al %s → %s', (formato, desde, hasta, esperado) => {
    // Arrange y Act
    const nombre = nombreArchivo(formato, desde, hasta)

    // Assert
    expect(nombre).toBe(esperado)
  })
})
