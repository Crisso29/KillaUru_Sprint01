package pe.killauru.app.contratos

import org.junit.Test
import pe.killauru.app.soporte.ValidadorContrato
import kotlin.test.assertTrue

/** T017 / FR-078: presencia y parseo de los contratos; no valida sus ejemplos. */
class ValidadorContratoTest {
    private val contratos = listOf(
        "001-plataforma-backend/contracts/plataforma.openapi.yaml",
        "003-modulo-identidad-accesos/contracts/identidad.openapi.yaml",
        "005-modulo-ingesta/contracts/ingesta.openapi.yaml",
        "006-modulo-consulta-mapa/contracts/mapa.openapi.yaml",
        "008-app-android/contracts/cdn.openapi.yaml",
    )

    @Test
    fun `todos los contratos esperados existen y se pueden parsear`() {
        // Arrange
        val faltantes = contratos.filterNot(ValidadorContrato::estaPresente)
        // Act / Assert
        assertTrue(faltantes.isEmpty(), "Faltan contratos: $faltantes. Ejecutar copiarContratos.")
        contratos.forEach { nombre ->
            val resultado = runCatching { ValidadorContrato.cargarYaml(nombre) }
            assertTrue(resultado.isSuccess, "No se pudo parsear '$nombre': ${resultado.exceptionOrNull()?.message}")
        }
    }
}
