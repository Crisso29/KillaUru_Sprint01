import { describe, it, expect } from 'vitest'
import { leerSesionPanel } from './sesionPanel'

// Contrato 003 `SesionPanel` (respuesta de inicio-sesion y renovar con canal = panel). Patrón AAA.
describe('leerSesionPanel', () => {
  it('lee las dos horas que manda el servidor', () => {
    // Arrange
    const json = { expiraAccesoUtc: '2026-11-22T10:05:00Z', sesionVenceUtc: '2026-11-22T18:00:00Z', rol: 'Administrador' }
    // Act
    const sesion = leerSesionPanel(json)
    // Assert
    expect(sesion.expiraAccesoUtc.toISOString()).toBe('2026-11-22T10:05:00.000Z')
    expect(sesion.sesionVenceUtc.toISOString()).toBe('2026-11-22T18:00:00.000Z')
  })

  it('rechaza una respuesta sin las horas del contrato (p. ej. el formato viejo acceso_vence_utc)', () => {
    // Arrange
    const viejo = { acceso_vence_utc: '2026-11-22T10:05:00Z' }

    // Act
    const leer = () => leerSesionPanel(viejo)

    // Assert — sin esto la sesión quedaba con «Invalid Date» y el panel no sabía cuándo renovar
    expect(leer).toThrow(/SesionPanel/)
  })
})
