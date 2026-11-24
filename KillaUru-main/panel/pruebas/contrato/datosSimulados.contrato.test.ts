// T011: los datos simulados (MSW) tienen la forma que espera el panel; si no, las pruebas de componentes y la
// demostración mostrarían pantallas que el backend real nunca produciría.
import { describe, it, expect } from 'vitest'
import datosAnomalias from '../msw/datos/anomalias.json'
import datosDashboard from '../msw/datos/dashboard.json'
import datosAuditoria from '../msw/datos/auditoria.json'

/** Fecha y hora UTC en ISO 8601 (contrato 002): «2026-11-05T08:00:00Z», con milisegundos opcionales. */
const ISO_UTC = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z$/

describe('Datos simulados MSW — forma básica', () => {
  it('anomalias es una FeatureCollection GeoJSON con al menos una anomalía', () => {
    // Arrange y Act
    const { type, features } = datosAnomalias

    // Assert
    expect(type).toBe('FeatureCollection')
    expect(features.length).toBeGreaterThan(0)
  })

  it('cada anomalía tiene id, estado, tipo, km y una severidad de 1 a 5', () => {
    // Arrange
    const anomalias = datosAnomalias.features

    // Act
    const invalidas = anomalias.filter(({ type, properties: anomalia }) => type !== 'Feature' || !anomalia.id || !anomalia.estado
      || !anomalia.tipo || typeof anomalia.kmVia !== 'number' || anomalia.severidad < 1 || anomalia.severidad > 5)

    // Assert
    expect(invalidas).toEqual([])
  })

  it('dashboard tiene indicadores y eventosPorDia', () => {
    // Arrange y Act
    const resumen = datosDashboard as { indicadores?: unknown; eventosPorDia?: unknown }

    // Assert
    expect(resumen.indicadores).toBeDefined()
    expect(Array.isArray(resumen.eventosPorDia)).toBe(true)
  })

  it('cada registro de auditoría tiene ocurridoUtc en ISO 8601 UTC y es una fecha real', () => {
    // Arrange
    const registros = (datosAuditoria as { elementos: Array<{ ocurridoUtc?: unknown }> }).elementos

    // Act — `new Date('cualquier cosa')` no lanza error (da «Invalid Date»): se valida el formato y la fecha
    const invalidos = registros.filter(({ ocurridoUtc }) =>
      typeof ocurridoUtc !== 'string' || !ISO_UTC.test(ocurridoUtc) || Number.isNaN(Date.parse(ocurridoUtc)))

    // Assert
    expect(registros.length).toBeGreaterThan(0)
    expect(invalidos).toEqual([])
  })
})
