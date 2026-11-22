package pe.killauru.app.sesion.ui

import android.Manifest.permission.ACCESS_BACKGROUND_LOCATION
import android.Manifest.permission.ACCESS_COARSE_LOCATION
import android.Manifest.permission.ACCESS_FINE_LOCATION
import org.junit.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/**
 * FR-006: la ubicación se pide en dos pasos. Desde Android 11 el sistema ignora «todo el tiempo» si se pide junto con
 * la precisa, así que primero la precisa y, ya concedida, la de segundo plano (Android 10+). Patrón AAA.
 */
class PermisosUbicacionTest {
    @Test fun `sin ubicacion precisa pide primero la precisa`() {
        // Arrange + Act
        val pedir = PermisosUbicacion.siguientes(tienePrecisa = false, tieneFondo = false, sdk = 34)
        // Assert
        assertContentEquals(arrayOf(ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION), pedir)
    }

    @Test fun `con la precisa en android 10 o mas pide la de segundo plano`() {
        // Arrange + Act
        val pedir = PermisosUbicacion.siguientes(tienePrecisa = true, tieneFondo = false, sdk = 29)
        // Assert
        assertContentEquals(arrayOf(ACCESS_BACKGROUND_LOCATION), pedir)
    }

    @Test fun `en android 9 la precisa ya basta`() {
        // Arrange + Act
        val completa = PermisosUbicacion.concedida(tienePrecisa = true, tieneFondo = false, sdk = 28)
        // Assert
        assertTrue(completa)
    }

    @Test fun `en android 10 o mas sin segundo plano la ubicacion esta incompleta`() {
        // Arrange + Act
        val completa = PermisosUbicacion.concedida(tienePrecisa = true, tieneFondo = false, sdk = 34)
        // Assert
        assertFalse(completa)
    }
}
