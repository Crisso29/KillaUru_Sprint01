package pe.killauru.app.comun.datos.bd

import org.junit.Test
import pe.killauru.app.deteccion.dominio.EstadoLocal
import pe.killauru.app.deteccion.dominio.TipoAnomalia
import pe.killauru.app.mapa.dominio.Alcance
import kotlin.test.assertEquals

/** T015 / FR-019, data-model §4: los códigos enteros nunca cambian (rotura de migración). */
class CodigosEstablesTest {
    @Test
    fun `EstadoLocal tiene los codigos del data-model`() {
        // Arrange / Act / Assert
        assertEquals(0, EstadoLocal.PENDING.codigo)
        assertEquals(1, EstadoLocal.ENVIANDO.codigo)
        assertEquals(2, EstadoLocal.SYNCED.codigo)
        assertEquals(3, EstadoLocal.REJECTED_LOCAL.codigo)
        assertEquals(4, EstadoLocal.EXPIRADO_LOCAL.codigo)
    }

    @Test
    fun `TipoAnomalia tiene los codigos del data-model`() {
        // Arrange / Act / Assert
        assertEquals(1, TipoAnomalia.Bache.codigo)
        assertEquals(2, TipoAnomalia.Hundimiento.codigo)
    }

    @Test
    fun `Alcance tiene los codigos del data-model`() {
        // Arrange / Act / Assert
        assertEquals(0, Alcance.Anonimo.codigo)
        assertEquals(1, Alcance.Registrado.codigo)
    }
}
