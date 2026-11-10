package pe.killauru.app.mapa.dominio

import org.junit.Test
import pe.killauru.app.comun.dominio.Coordenada
import kotlin.test.assertFalse
import kotlin.test.assertTrue

/**
 * T083 / FR-049, R-035: aviso de «fuera del tramo demo» stateful.
 * Patrón AAA en cada test.
 */
class AvisoFueraDemoTest {

    // Polígono mínimo de 4 vértices alrededor del punto (0, 0)
    private val zonaInterior = ZonaDemo(
        listOf(
            Coordenada(-1.0, -1.0),
            Coordenada(-1.0, 1.0),
            Coordenada(1.0, 1.0),
            Coordenada(1.0, -1.0),
        )
    )

    private val dentroDeLaZona = Coordenada(0.0, 0.0)
    private val fueraDeLaZona = Coordenada(5.0, 5.0)

    @Test fun `primera vez fuera en el viaje emite aviso`() {
        // Arrange
        val aviso = AvisoFueraDemo(zonaInterior)
        // Act
        val resultado = aviso.debeAvisar(fueraDeLaZona, Alcance.Anonimo)
        // Assert
        assertTrue(resultado)
    }

    @Test fun `segunda vez fuera en el mismo viaje no emite aviso`() {
        // Arrange
        val aviso = AvisoFueraDemo(zonaInterior)
        aviso.debeAvisar(fueraDeLaZona, Alcance.Anonimo)
        // Act
        val resultado = aviso.debeAvisar(fueraDeLaZona, Alcance.Anonimo)
        // Assert
        assertFalse(resultado, "ya se avisó en este viaje; no debe repetir")
    }

    @Test fun `viaje nuevo resetea el aviso`() {
        // Arrange
        val aviso = AvisoFueraDemo(zonaInterior)
        aviso.debeAvisar(fueraDeLaZona, Alcance.Anonimo)  // primera vez
        aviso.iniciarViaje()                                // nuevo viaje
        // Act
        val resultado = aviso.debeAvisar(fueraDeLaZona, Alcance.Anonimo)
        // Assert
        assertTrue(resultado, "tras un nuevo viaje debe volver a avisar")
    }

    @Test fun `usuario registrado nunca recibe aviso`() {
        // Arrange
        val aviso = AvisoFueraDemo(zonaInterior)
        // Act
        val resultado = aviso.debeAvisar(fueraDeLaZona, Alcance.Registrado)
        // Assert
        assertFalse(resultado, "los usuarios registrados ven el mapa completo; sin aviso")
    }

    @Test fun `dentro de la zona nunca emite aviso`() {
        // Arrange
        val aviso = AvisoFueraDemo(zonaInterior)
        // Act
        val resultado = aviso.debeAvisar(dentroDeLaZona, Alcance.Anonimo)
        // Assert
        assertFalse(resultado)
    }
}
