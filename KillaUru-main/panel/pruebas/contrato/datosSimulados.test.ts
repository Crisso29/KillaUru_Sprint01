import { describe, it, expect } from 'vitest'
import datosAnomalias from '../msw/datos/anomalias.json'
import datosDashboard from '../msw/datos/dashboard.json'
import datosAuditoria from '../msw/datos/auditoria.json'

// T011: los datos simulados tienen la forma esperada por el panel
describe('Datos simulados MSW — forma básica', () => {
  it('anomalias tiene items y total', () => {
    expect(datosAnomalias).toHaveProperty('items')
    expect(datosAnomalias).toHaveProperty('total')
    expect(Array.isArray(datosAnomalias.items)).toBe(true)
  })

  it('cada anomalía tiene campos requeridos', () => {
    for (const a of datosAnomalias.items) {
      expect(a).toHaveProperty('id')
      expect(a).toHaveProperty('estado')
      expect(a).toHaveProperty('severidad')
      expect(a['severidad']).toBeGreaterThanOrEqual(1)
      expect(a['severidad']).toBeLessThanOrEqual(5)
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
