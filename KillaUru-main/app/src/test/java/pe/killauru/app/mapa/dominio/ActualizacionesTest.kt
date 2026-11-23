package pe.killauru.app.mapa.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.actualizador.dominio.ActualizarModeloUseCase
import pe.killauru.app.actualizador.dominio.ManifiestoModelo
import pe.killauru.app.actualizador.dominio.PoliticaActivacionModelo
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.assertNull

/**
 * FR-071, RF-33: el modelo nuevo solo se activa si la app es compatible y no se reactiva si ya está. (La descarga del
 * mapa está en `ActualizarMapaUseCaseTest`.) Patrón AAA.
 */
class ActualizacionesTest {
    private val prefs = PreferenciasMemoria()
    private val caso = ActualizarModeloUseCase(prefs, PoliticaActivacionModelo("1.2.0"))
    private val modelo = ManifiestoModelo("2", "modelo.tflite", 1, listOf("Normal", "Bache", "Hundimiento"), 5, "1.0.0")

    @Test fun `un modelo que pide una app mas nueva no se activa`() = runTest {
        // Arrange + Act
        val r = caso.evaluar(modelo.copy(versionMinimaApp = "9.0"))
        // Assert
        assertIs<ActualizarModeloUseCase.Resultado.IncompatibleConApp>(r)
        assertNull(prefs.modeloVersion)
    }

    @Test fun `un modelo compatible se activa`() = runTest {
        // Arrange + Act
        val r = caso.evaluar(modelo)
        // Assert
        assertIs<ActualizarModeloUseCase.Resultado.Activado>(r)
        assertEquals("2", prefs.modeloVersion)
    }

    @Test fun `el mismo modelo ya activo no se vuelve a activar`() = runTest {
        // Arrange
        caso.evaluar(modelo)
        // Act
        val r = caso.evaluar(modelo)
        // Assert
        assertIs<ActualizarModeloUseCase.Resultado.SinCambios>(r)
    }
}
