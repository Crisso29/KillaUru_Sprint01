package pe.killauru.app.historial.ui

import org.junit.Test
import kotlin.test.assertEquals

/** Prototipo «Mis viajes»: «Ayacucho → Vinchos», «km 333 → 268 · 1 h 20 min». Patrón AAA. */
class TextosViajeTest {
    @Test fun `un km cerca de un pueblo toma su nombre`() {
        // Arrange + Act + Assert
        assertEquals("Ayacucho", TextosViaje.lugar(333.0))
        assertEquals("Vinchos", TextosViaje.lugar(270.4))
        assertEquals("Pisco", TextosViaje.lugar(3.0))
    }

    @Test fun `lejos de todo pueblo se dice el km`() {
        // Arrange + Act + Assert
        assertEquals("km 90", TextosViaje.lugar(90.2))
    }

    @Test fun `la duracion va en horas y minutos`() {
        // Arrange — 1 h 20 min y 45 min
        val unaHoraVeinte = 80 * 60_000L
        // Act + Assert
        assertEquals("1 h 20 min", TextosViaje.duracion(0, unaHoraVeinte))
        assertEquals("45 min", TextosViaje.duracion(0, 45 * 60_000L))
    }

    @Test fun `el detalle junta los km y la duracion`() {
        // Arrange + Act + Assert
        assertEquals("km 333 → 268 · 1 h 20 min", TextosViaje.detalle(333.0, 268.0, 0, 80 * 60_000L))
    }
}
