// T082 — nombreArchivo: genera nombre de archivo por formato
import { describe, it, expect } from 'vitest'
import { nombreArchivo } from './descarga'

describe('nombreArchivo', () => {
  it('CSV → extensión .csv', () => {
    expect(nombreArchivo('CSV', '2026-10-13', '2026-10-20')).toBe(
      'killauru-anomalias-20261013-20261020.csv',
    )
  })

  it('GeoJSON → extensión .geojson', () => {
    expect(nombreArchivo('GeoJSON', '2026-10-13', '2026-10-20')).toBe(
      'killauru-anomalias-20261013-20261020.geojson',
    )
  })

  it('sin fechas → solo prefijo con extensión', () => {
    expect(nombreArchivo('CSV', '', '')).toBe('killauru-anomalias.csv')
  })
})
