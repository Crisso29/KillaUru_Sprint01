package pe.killauru.app.sesion.dominio

import kotlinx.coroutines.runBlocking
import org.junit.Test
import pe.killauru.app.sesion.dominio.casos.RevisarVersionesLegalesUseCase
import kotlin.test.assertIs

/** T026 / FR-002, FR-003, R-024: versiones legales con y sin red. */
class RevisarVersionesLegalesUseCaseTest {

    private class PrefsDoble(
        private val terminosAceptado: String? = null,
        private val politicaAceptada: String? = null,
    ) : RepositorioPreferencias {
        override suspend fun obtenerTerminosVersion() = terminosAceptado
        override suspend fun guardarTerminosVersion(version: String) {}
        override suspend fun obtenerTerminosAceptadoUtc(): Long? = null
        override suspend fun guardarTerminosAceptadoUtc(utc: Long) {}
        override suspend fun obtenerPoliticaVersion() = politicaAceptada
        override suspend fun guardarPoliticaVersion(version: String) {}
        override suspend fun obtenerPoliticaAceptadoUtc(): Long? = null
        override suspend fun guardarPoliticaAceptadoUtc(utc: Long) {}
        override suspend fun obtenerCategoria(): CategoriaVehiculo? = null
        override suspend fun guardarCategoria(categoria: CategoriaVehiculo) {}
        override suspend fun obtenerSoloVerMapa() = false
        override suspend fun guardarSoloVerMapa(valor: Boolean) {}
        override suspend fun obtenerUsarDatosMoviles() = false
        override suspend fun guardarUsarDatosMoviles(valor: Boolean) {}
        override suspend fun obtenerSeudonimo(): String? = null
        override suspend fun guardarSeudonimo(seudonimo: String, desdeUtc: Long) {}
        override suspend fun obtenerSeudonimoDesdeUtc(): Long? = null
        override suspend fun obtenerBackoffPaso() = 0
        override suspend fun guardarBackoffPaso(paso: Int) {}
        override suspend fun obtenerMapaVersion(): String? = null
        override suspend fun guardarMapaVersion(version: String) {}
        override suspend fun obtenerModeloVersion(): String? = null
        override suspend fun guardarModeloVersion(version: String) {}
        override suspend fun limpiarSesion() {}
    }

    @Test
    fun `versiones iguales a las aceptadas devuelve Vigente`() {
        // Arrange
        val prefs = PrefsDoble("1.0", "1.0")
        val uc = RevisarVersionesLegalesUseCase(prefs, "1.0", "1.0")
        // Act
        val resultado = runBlocking { uc.revisar() }
        // Assert
        assertIs<ResultadoVersiones.Vigente>(resultado)
    }

    @Test
    fun `terminos sin aceptar devuelve PedirAceptacion`() {
        // Arrange
        val prefs = PrefsDoble(terminosAceptado = null, politicaAceptada = "1.0")
        val uc = RevisarVersionesLegalesUseCase(prefs, "1.0", "1.0")
        // Act
        val resultado = runBlocking { uc.revisar() }
        // Assert
        assertIs<ResultadoVersiones.PedirAceptacion>(resultado)
    }

    @Test
    fun `version diferente a la aceptada devuelve PedirAceptacion`() {
        // Arrange
        val prefs = PrefsDoble("1.0", "1.0")
        val uc = RevisarVersionesLegalesUseCase(prefs, "2.0", "1.0")
        // Act
        val resultado = runBlocking { uc.revisar() }
        // Assert
        assertIs<ResultadoVersiones.PedirAceptacion>(resultado)
    }

    @Test
    fun `sin red version del APK sirve como fuente de verdad`() {
        // Arrange - sin servidor; APK tiene 1.0 y usuario ya acepto 1.0
        val prefs = PrefsDoble("1.0", "1.0")
        val uc = RevisarVersionesLegalesUseCase(prefs, terminosApk = "1.0", politicaApk = "1.0")
        // Act
        val resultado = runBlocking { uc.revisar() }
        // Assert
        assertIs<ResultadoVersiones.Vigente>(resultado)
    }
}
