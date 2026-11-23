package pe.killauru.app.sincronizacion.datos

import androidx.room.Room
import androidx.test.core.app.ApplicationProvider
import kotlinx.coroutines.test.runTest
import org.junit.After
import org.junit.Before
import org.junit.Test
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner
import pe.killauru.app.comun.datos.bd.KillaUruBd
import pe.killauru.app.soporte.sinHilos
import kotlin.test.assertEquals

/**
 * FR-031, data-model §1 (`contador_local`): los rechazos se **acumulan** por motivo con la clave `rechazo.<Motivo>`.
 * Usa Room real en memoria: lo que se prueba es la sentencia SQL, no un doble.
 */
@RunWith(RobolectricTestRunner::class)
class ContadorRechazosRoomTest {
    private lateinit var bd: KillaUruBd
    private lateinit var contador: ContadorRechazosRoom

    @Before
    fun setUp() {
        bd = Room.inMemoryDatabaseBuilder(ApplicationProvider.getApplicationContext(), KillaUruBd::class.java)
            .sinHilos()
            .build()
        contador = ContadorRechazosRoom(bd.contadorDao())
    }

    @After
    fun tearDown() = bd.close()

    @Test
    fun `motivos repetidos en un mismo lote se suman`() = runTest {
        // Arrange
        val motivos = listOf("FueraDelCorredor", "FueraDelCorredor", "EsquemaInvalido")

        // Act
        contador.sumar(motivos)

        // Assert
        assertEquals(2, bd.contadorDao().obtener("rechazo.FueraDelCorredor"))
        assertEquals(1, bd.contadorDao().obtener("rechazo.EsquemaInvalido"))
    }

    @Test
    fun `rechazos de lotes distintos se acumulan`() = runTest {
        // Arrange
        contador.sumar(listOf("ConfidenceBajo"))

        // Act
        contador.sumar(listOf("ConfidenceBajo"))

        // Assert
        assertEquals(2, bd.contadorDao().obtener("rechazo.ConfidenceBajo"))
    }
}
