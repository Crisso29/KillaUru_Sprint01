package pe.killauru.app.mapa.dominio

import kotlinx.coroutines.test.runTest
import org.junit.Test
import pe.killauru.app.actualizador.dominio.*
import pe.killauru.app.soporte.PreferenciasMemoria
import kotlin.test.*

class ActualizacionesTest {
    @Test fun `mapa respeta wifi datos moviles y version instalada`() = runTest {
        val prefs = PreferenciasMemoria()
        val caso = ActualizarMapaUseCase(prefs, PoliticaDescarga())
        assertIs<ActualizarMapaUseCase.Resultado.SinCambios>(caso.evaluar("2", false))
        assertIs<ActualizarMapaUseCase.Resultado.DescargaPendiente>(caso.evaluar("2", true))
        prefs.usarDatosMoviles = true
        assertIs<ActualizarMapaUseCase.Resultado.DescargaPendiente>(caso.evaluar("2", false))
        caso.marcarDescargado("2")
        assertEquals("2", prefs.mapaVersion)
        assertIs<ActualizarMapaUseCase.Resultado.SinCambios>(caso.evaluar("2", true))
    }

    @Test fun `solo activa modelos compatibles y evita cambios redundantes`() = runTest {
        val prefs = PreferenciasMemoria()
        val caso = ActualizarModeloUseCase(prefs, PoliticaActivacionModelo("1.2.0"))
        val modelo = ManifiestoModelo("2", "modelo.tflite", 1,
            listOf("Normal", "Bache", "Hundimiento"), 5, "1.0.0")
        assertIs<ActualizarModeloUseCase.Resultado.IncompatibleConApp>(caso.evaluar(modelo.copy(versionMinimaApp = "9.0")))
        assertNull(prefs.modeloVersion)
        assertIs<ActualizarModeloUseCase.Resultado.Activado>(caso.evaluar(modelo))
        assertEquals("2", prefs.modeloVersion)
        assertIs<ActualizarModeloUseCase.Resultado.SinCambios>(caso.evaluar(modelo))
    }
}
