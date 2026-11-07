package pe.killauru.app.soporte

import org.junit.Assume.assumeTrue
import org.junit.Test
import kotlin.test.assertTrue

/** T017 / FR-078: los YAML deben ser parseables y sus ejemplos válidos. */
class ValidadorContratoTest {
    private val contratos = listOf(
        "001-requisitos-funcionales/contracts/ingesta.openapi.yaml",
        "003-diseno-interno/contracts/identidad.openapi.yaml",
        "005-modulo-ingesta/contracts/ingesta.openapi.yaml",
        "006-modulo-consulta-mapa/contracts/mapa.openapi.yaml",
        "cdn.openapi.yaml",
    )

    @Test
    fun `los YAML presentes se pueden parsear con swagger-parser`() {
        // Arrange
        val presentes = contratos.filter(ValidadorContrato::estaPresente)
        // Act / Assert
        assumeTrue("No hay YAMLs copiados (ejecutar con copiarContratos)", presentes.isNotEmpty())
        presentes.forEach { nombre ->
            val resultado = runCatching { ValidadorContrato.cargarYaml(nombre) }
            assertTrue(resultado.isSuccess, "No se pudo parsear '$nombre': ${resultado.exceptionOrNull()?.message}")
        }
    }
}
