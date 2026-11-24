// T072 / T047 — «qué pasó» de un cambio de estado: tabla completa de data-model §5 (FR-020, FR-039, R-014).
// Una sola tabla para el Resumen («Últimos cambios») y el Historial del detalle: antes había dos copias que ya diferían.
import { describe, it, expect } from 'vitest'
import { textoQuePaso } from './transicion'

const ACTOR = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

describe('textoQuePaso', () => {
  it.each([
    [null, 'Activa', null, 'Creada con el primer reporte'],
    [null, 'PendienteConfirmacion', null, 'Nueva: espera confirmación'],
    ['PendienteConfirmacion', 'Activa', null, 'Confirmada por 2.º vehículo'],
    ['Activa', 'Expirada', null, 'Expiró sin reportes nuevos'],
    ['PendienteConfirmacion', 'Expirada', null, 'Expiró sin reportes nuevos'],
    ['Expirada', 'Activa', null, 'Volvió con un reporte nuevo'],
    ['Reparada', 'Activa', null, 'Volvió con un reporte nuevo'],
    ['Expirada', 'PendienteConfirmacion', null, 'Volvió con un reporte grave: espera confirmación'],
    ['Reparada', 'PendienteConfirmacion', null, 'Volvió con un reporte grave: espera confirmación'],
    ['Activa', 'Reparada', ACTOR, 'Marcada reparada'],
    ['Activa', 'Descartada', ACTOR, 'Descartada'],
    ['Reparada', 'Activa', ACTOR, 'Reabierta'],
    ['Descartada', 'Activa', ACTOR, 'Reabierta'],
  ])('%s → %s (actor: %s) → «%s»', (desde, hacia, actorId, esperado) => {
    // Arrange y Act
    const texto = textoQuePaso(desde, hacia, actorId)

    // Assert
    expect(texto).toBe(esperado)
  })

  it('una combinación fuera de la tabla nombra el estado en lenguaje claro, nunca con su código', () => {
    // Arrange
    const desde = 'Activa'
    const hacia = 'PendienteConfirmacion'

    // Act
    const texto = textoQuePaso(desde, hacia, ACTOR)

    // Assert
    expect(texto).toBe('Cambió a pendiente de confirmar')
  })
})
