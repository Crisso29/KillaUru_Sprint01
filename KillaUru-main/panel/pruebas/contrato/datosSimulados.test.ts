import { describe, it, expect } from 'vitest'
import datosAnomalias from '../msw/datos/anomalias.json'
import datosDashboard from '../msw/datos/dashboard.json'
import datosAuditoria from '../msw/datos/auditoria.json'

// T011: los datos simulados tienen la forma esperada por el panel
describe('Datos simulados MSW — forma básica', () => {
  it('anomalias es una FeatureCollection GeoJSON', () => {
    expect(datosAnomalias.type).toBe('FeatureCollection')
    expect(Array.isArray(datosAnomalias.features)).toBe(true)
    expect(datosAnomalias.features.length).toBeGreaterThan(0)
  })

  it('cada feature tiene propiedades requeridas', () => {
    for (const f of datosAnomalias.features) {
      expect(f.type).toBe('Feature')
      expect(f.properties).toHaveProperty('id')
      expect(f.properties).toHaveProperty('estado')
      expect(f.properties).toHaveProperty('severidad')
      expect(f.properties.severidad).toBeGreaterThanOrEqual(1)
      expect(f.properties.severidad).toBeLessThanOrEqual(5)
      expect(f.properties).toHaveProperty('kmVia')
      expect(f.properties).toHaveProperty('tipo')
    }
  })

  it('dashboard tiene total_senales y senales_por_dia', () => {
    expect(datosDashboard).toHaveProperty('total_senales')
    expect(Array.isArray(datosDashboard.senales_por_dia)).toBe(true)
  })

  it('auditoria tiene items con ts_utc en ISO 8601', () => {
    for (const e of datosAuditoria.items) {
      expect(e).toHaveProperty('ts_utc')
      expect(() => new Date(e['ts_utc'])).not.toThrow()
    }
  })
})
