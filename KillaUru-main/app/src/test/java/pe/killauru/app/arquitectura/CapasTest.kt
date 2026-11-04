package pe.killauru.app.arquitectura

import com.lemonappdev.konsist.api.Konsist
import org.junit.Test
import kotlin.test.assertTrue

/** T007 / FR-075: inspección de imports con Konsist, sin excepciones por funcionalidad. */
class CapasTest {
    @Test
    fun `dominio no conoce frameworks ni capas externas`() {
        // Arrange
        val archivos = Konsist.scopeFromProject().files.filter { "/dominio/" in it.path }
        val prohibidos = listOf("android.", "androidx.", "retrofit2.", "okhttp3.",
            "org.maplibre.", "com.google.ai.edge.", "com.google.dagger.")
        // Act
        val errores = archivos.flatMap { archivo -> archivo.imports.map { it.name } }
            .filter { nombre -> prohibidos.any(nombre::startsWith) ||
                ".datos." in nombre || ".ui." in nombre }
        // Assert
        assertTrue(errores.isEmpty(), errores.joinToString())
    }

    @Test
    fun `presentacion y datos no se importan mutuamente`() {
        // Arrange
        val archivos = Konsist.scopeFromProject().files
        // Act
        val errores = archivos.flatMap { archivo -> archivo.imports.mapNotNull { dependencia ->
            dependencia.name.takeIf { ("/ui/" in archivo.path && ".datos." in it) ||
                ("/datos/" in archivo.path && ".ui." in it) }
        } }
        // Assert
        assertTrue(errores.isEmpty(), errores.joinToString())
    }
}
