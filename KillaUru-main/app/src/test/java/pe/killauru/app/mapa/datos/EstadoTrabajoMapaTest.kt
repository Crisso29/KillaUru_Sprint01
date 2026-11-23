package pe.killauru.app.mapa.datos

import androidx.work.Data
import androidx.work.WorkInfo
import androidx.work.workDataOf
import org.junit.Test
import pe.killauru.app.mapa.dominio.EstadoDescarga
import java.util.UUID
import kotlin.test.assertEquals

/** FR-041, FR-043: cómo se traduce el trabajo de WorkManager a lo que ve el conductor en el paso 4. Patrón AAA. */
class EstadoTrabajoMapaTest {
    private fun trabajo(estado: WorkInfo.State, intentos: Int = 0, progreso: Data = Data.EMPTY, salida: Data = Data.EMPTY) =
        WorkInfo(UUID.randomUUID(), estado, emptySet(), salida, progreso, intentos)

    @Test fun `sin trabajo o cancelado es que aun no empezo`() {
        // Arrange + Act + Assert
        assertEquals(EstadoDescarga.SinEmpezar, estadoDescarga(null))
        assertEquals(EstadoDescarga.SinEmpezar, estadoDescarga(trabajo(WorkInfo.State.CANCELLED)))
    }

    @Test fun `corriendo muestra el porcentaje y el peso que informo el trabajo`() {
        // Arrange
        val corriendo = trabajo(WorkInfo.State.RUNNING, progreso = workDataOf(
            DescargarMapaWorker.CLAVE_PROGRESO to 62, DescargarMapaWorker.CLAVE_BYTES to 2_362_855L))
        // Act
        val estado = estadoDescarga(corriendo)
        // Assert
        assertEquals(EstadoDescarga.Descargando(62, 2_362_855L), estado)
    }

    @Test fun `en cola tras un intento fallido es que se esta reintentando`() {
        // Arrange + Act + Assert — «Sin conexión. Seguirá solo cuando vuelva la señal.»
        assertEquals(EstadoDescarga.Reintentando, estadoDescarga(trabajo(WorkInfo.State.ENQUEUED, intentos = 1)))
        assertEquals(EstadoDescarga.Descargando(0), estadoDescarga(trabajo(WorkInfo.State.ENQUEUED)))
    }

    @Test fun `terminado con bytes en espera es que espera wifi, y sin ellos que esta listo`() {
        // Arrange
        val espera = trabajo(WorkInfo.State.SUCCEEDED, salida = workDataOf(DescargarMapaWorker.CLAVE_ESPERA_BYTES to 2_362_855L))
        // Act + Assert
        assertEquals(EstadoDescarga.EsperandoWifi(2_362_855L), estadoDescarga(espera))
        assertEquals(EstadoDescarga.Listo, estadoDescarga(trabajo(WorkInfo.State.SUCCEEDED)))
    }
}
