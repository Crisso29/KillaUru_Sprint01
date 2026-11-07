package pe.killauru.app.sesion.dominio

import org.junit.Test
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/** T106 / FR-063, RN-21: permiso local vigente hasta sesionVenceUtc. */
class PermisoLocalTest {

    @Test
    fun `permiso dentro de vigencia es valido`() {
        // Arrange
        val ahoraMs = 1000L
        val permiso = PermisoLocal(venceMs = 2000L)
        // Act / Assert
        assertTrue(permiso.vigente(ahoraMs))
    }

    @Test
    fun `permiso vencido sin viaje activo no es valido`() {
        // Arrange
        val ahoraMs = 3000L
        val permiso = PermisoLocal(venceMs = 2000L)
        // Act / Assert
        assertFalse(permiso.vigente(ahoraMs, viajeActivo = false))
    }

    @Test
    fun `permiso vencido durante viaje activo sigue valido`() {
        // Arrange
        val ahoraMs = 3000L
        val permiso = PermisoLocal(venceMs = 2000L)
        // Act / Assert
        assertTrue(permiso.vigente(ahoraMs, viajeActivo = true))
    }

    @Test
    fun `permiso vencido hace mas de 30min con viaje activo no es valido`() {
        // Arrange
        val ahoraMs = 2000L + (31 * 60 * 1000L)
        val permiso = PermisoLocal(venceMs = 2000L)
        // Act / Assert
        assertFalse(permiso.vigente(ahoraMs, viajeActivo = true))
    }
}
